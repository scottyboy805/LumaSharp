using LumaSharp.Compiler;
using Xunit;

namespace LumaSharp.Tests;

public sealed class AsyncFeatureTests
{
    [Fact]
    public async Task AsyncVirtualContractsSupportDirectForwardingAndAwaitValueReturns()
    {
        const string source = """
using System.Threading;

public virtual class AsyncBase
{
    public virtual async int32 GetValue();
}

public class AsyncDerived : AsyncBase
{
    public override async int32 GetValue()
    {
        return Program.GetSharedTask();
    }
}

class Program
{
    private static Task<int32> shared = CalculateAsync();

    public static Task<int32> GetSharedTask()
    {
        return shared;
    }

    public static async int32 CalculateAsync()
    {
        return 42;
    }

    public static async int32 ForwardAsync()
    {
        return CalculateAsync();
    }

    public static async int32 ForwardTaskAsync(Task<int32> pending)
    {
        return pending;
    }

    public static async int32 AwaitOrValueAsync(bool useAsync)
    {
        if (useAsync)
            return await CalculateAsync();
        return 9;
    }

    public static int32 Main()
    {
        shared = CalculateAsync();
        Task<int32> forwarded = ForwardAsync();
        Task<int32> parameterForwarded = ForwardTaskAsync(shared);
        AsyncBase instance = new AsyncDerived();
        Task<int32> virtualResult = instance.GetValue();

        if (forwarded.Result != 42 || parameterForwarded != shared || virtualResult != shared)
            return 1;
        if (AwaitOrValueAsync(true).Result != 42 || AwaitOrValueAsync(false).Result != 9)
            return 2;
        return 0;
    }
}
""";

        const string abstractStreamInstantiation = """
using System.IO;
class Program
{
    public static int32 Main()
    {
        AsyncStream stream = new AsyncStream();
        return 0;
    }
}
""";
        var abstractStream = CompileWithRuntime(abstractStreamInstantiation, "async-stream-is-abstract.ls");
        Assert.False(abstractStream.Succeeded);
        Assert.Contains(abstractStream.Diagnostics, diagnostic => diagnostic.Code == "LS0834");

        var execution = await BuildAndRunNativeAsync(source, "AsyncVirtualForwarding");
        Assert.DoesNotContain("luma_Program___hidden_ForwardAsync_async_body_", execution.LlvmIr, StringComparison.Ordinal);
        Assert.DoesNotContain("luma_Program___hidden_ForwardTaskAsync_async_body_", execution.LlvmIr, StringComparison.Ordinal);
        Assert.Equal(0, execution.ExitCode);
        Assert.Empty(execution.StandardError);
        Assert.Empty(execution.StandardOutput);

        const string nonAsyncOverride = """
using System.Threading;

public virtual class AsyncBase
{
    public virtual async int32 GetValue();
}

class Program
{
    public static async int32 CalculateAsync() { return 1; }
    public static int32 Main() { return 0; }
}

class InvalidDerived : AsyncBase
{
    public override System.Threading.Task<int32> GetValue()
    {
        return Program.CalculateAsync();
    }
}
""";
        var invalidOverride = CompileWithRuntime(nonAsyncOverride, "async-nonasync-override.ls");
        Assert.False(invalidOverride.Succeeded);
        Assert.Contains(invalidOverride.Diagnostics, diagnostic => diagnostic.Code == "LS0832");

        const string nonAsyncInterfaceImplementation = """
using System.Threading;

public interface IAsyncValue
{
    async int32 GetValue();
}

class Program
{
    public static async int32 CalculateAsync() { return 1; }
    public static int32 Main() { return 0; }
}

public class InvalidValue : IAsyncValue
{
    public System.Threading.Task<int32> GetValue()
    {
        return Program.CalculateAsync();
    }
}
""";
        var invalidInterface = CompileWithRuntime(nonAsyncInterfaceImplementation, "async-nonasync-interface.ls");
        Assert.False(invalidInterface.Succeeded);
        Assert.Contains(invalidInterface.Diagnostics, diagnostic => diagnostic.Code == "LS1001");

        const string mixedModes = """
using System.Threading;

class Program
{
    public static async int32 ChildAsync() { return 1; }

    public static async int32 Invalid(bool useAwait)
    {
        if (useAwait)
            return await ChildAsync();
        return ChildAsync();
    }

    public static int32 Main() { return 0; }
}
""";
        var invalidMixedModes = CompileWithRuntime(mixedModes, "async-mixed-modes.ls");
        Assert.False(invalidMixedModes.Succeeded);
    }

    [Fact]
    public async Task AsyncAwaitNeedsNoTaskImportAndCallsCanBeFireAndForget()
    {
        const string noTaskImport = """
using System;

class Program
{
    public static async int32 Child()
    {
        return 7;
    }

    public static async int32 Parent()
    {
        return await Child();
    }

    public static int32 Main()
    {
        return Parent().Result == 7 ? 0 : 1;
    }
}
""";
        var awaited = await BuildAndRunNativeAsync(noTaskImport, "AsyncAwaitWithoutTaskImport");
        Assert.Equal(0, awaited.ExitCode);
        Assert.Empty(awaited.StandardError);

        const string source = """
using System;
using System.Threading;

class Program
{
    private static bool completed;

    public static async void CountToTen()
    {
        await Task.Delay(5);
        completed = true;
    }

    public static int32 Main()
    {
        CountToTen();
        int32 attempts = 0;
        while (!completed && attempts < 1000)
        {
            System.Threading.Thread.Sleep(1);
            attempts++;
        }
        return completed ? 0 : 1;
    }
}
""";

        var execution = await BuildAndRunNativeAsync(source, "AsyncFireAndForget");
        Assert.Equal(0, execution.ExitCode);
        Assert.Empty(execution.StandardError);

        const string instanceCallFromStaticMethod = """
class Program
{
    public async void CountToTen() { }
    public static int32 Main()
    {
        CountToTen();
        return 0;
    }
}
""";
        var invalidCall = CompileWithRuntime(instanceCallFromStaticMethod, "async-instance-call-from-static.ls");
        Assert.False(invalidCall.Succeeded);
        Assert.Contains(invalidCall.Diagnostics, diagnostic => diagnostic.Code == "LS0200" &&
            diagnostic.Message.Contains("requires an object receiver in a static method", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AwaitedDelayWaitsAndRetainedTaskKeepsAsyncWorkAlive()
    {
        const string source = """
using System.Diagnostics;
using System.Threading;

class Program
{
    public static async int32 CountAfterTwoDelays()
    {
        await Task.Delay(100);
        await Task.Delay(100);
        return 2;
    }

    public static int32 Main()
    {
        StopWatch watch = StopWatch.StartNew();
        Task<int32> task = CountAfterTwoDelays();
        int32 count = task.Result;
        return count == 2 && watch.Elapsed.TotalMilliseconds >= 150.0 ? 0 : 1;
    }
}
""";

        var execution = await BuildAndRunNativeAsync(source, "AsyncAwaitDelayTiming");
        Assert.Equal(0, execution.ExitCode);
        Assert.Empty(execution.StandardError);
    }

    [Fact]
    public async Task FileStreamAsyncReadAndWriteWorkThroughAsyncStream()
    {
        var filePath = Path.Combine(Path.GetTempPath(), "lumasharp-async-" + Guid.NewGuid().ToString("N") + ".bin");
        var escapedPath = filePath.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
        try
        {
            var source = $$"""
using System.IO;
using System.Threading;

class Program
{
    public static int32 Main()
    {
        AsyncStream writer = new FileStream("{{escapedPath}}", FileMode.Create);
        uint8[] input = { 10, 20, 30, 40, 50 };
        Task write = writer.WriteAsync(input, 1, 3);
        write.Wait();
        if (!write.IsCompleted)
            return 1;
        writer.Close();

        AsyncStream reader = new FileStream("{{escapedPath}}", FileMode.Open);
        uint8[] output = new uint8[5];
        Task<int32> read = reader.ReadAsync(output, 1, 4);
        int32 readCount = read.Result;
        if (readCount != 3 || output[0] != 0 || output[1] != 20 ||
            output[2] != 30 || output[3] != 40 || output[4] != 0)
            return 2;

        Task<int32> eof = reader.ReadAsync(output, 0, 1);
        try
        {
            if (eof.Result != 0)
                return 3;
        }
        catch (Exception error)
        {
            System.Console.WriteLine(error.ToString());
            return 5;
        }
        reader.Close();

        Task<int32> readAfterClose = reader.ReadAsync(output, 0, 1);
        try
        {
            int32 ignored = readAfterClose.Result;
            return 4;
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }
}
""";

            var execution = await BuildAndRunNativeAsync(source, "AsyncFileStream");
            Assert.Contains("__hidden_ReadAsync_async_body_", execution.LlvmIr, StringComparison.Ordinal);
            Assert.Contains("__hidden_WriteAsync_async_body_", execution.LlvmIr, StringComparison.Ordinal);
            if (OperatingSystem.IsWindows())
            {
                Assert.Contains("i32 1073741952", execution.LlvmIr);
                Assert.Contains("@GetOverlappedResult", execution.LlvmIr);
                Assert.Contains("@ReadFile", execution.LlvmIr);
                Assert.Contains("@WriteFile", execution.LlvmIr);
            }
            else
            {
                Assert.Contains("@fread", execution.LlvmIr);
                Assert.Contains("@fwrite", execution.LlvmIr);
            }
            Assert.True(execution.ExitCode == 0, $"Exit {execution.ExitCode}; stdout: {execution.StandardOutput}; stderr: {execution.StandardError}");
            Assert.Empty(execution.StandardError);
            Assert.Empty(execution.StandardOutput);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task FileStreamAsyncReadAndWriteObserveCancellationBetweenChunks()
    {
        var filePath = Path.Combine(Path.GetTempPath(), "lumasharp-async-cancel-" + Guid.NewGuid().ToString("N") + ".bin");
        var escapedPath = filePath.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
        var source = $$"""
using System;
using System.IO;
using System.Threading;

class DelayedFileStream : FileStream
{
    private static object gate = new object();
    private static bool readStarted;
    private static bool writeStarted;

    public this(string path, FileMode mode) : base(path, mode) { }

    public static void ResetRead()
    {
        lock (gate)
            readStarted = false;
    }

    public static bool ReadStarted
    {
        get
        {
            lock (gate)
                return readStarted;
        }
    }

    public static void ResetWrite()
    {
        lock (gate)
            writeStarted = false;
    }

    public static bool WriteStarted
    {
        get
        {
            lock (gate)
                return writeStarted;
        }
    }

    public override int32 Read(scope uint8[] buffer, int32 offset, int32 count) mut
    {
        lock (gate)
            readStarted = true;
        Thread.Sleep(50);
        return base.Read(buffer, offset, count);
    }

    public override void Write(scope uint8[] buffer, int32 offset, int32 count) mut
    {
        lock (gate)
            writeStarted = true;
        Thread.Sleep(50);
        base.Write(buffer, offset, count);
    }
}

class Program
{
    public static int32 Main()
    {
        uint8[] payload = new uint8[262144];
        FileStream initial = new FileStream("{{escapedPath}}", FileMode.Create);
        initial.Write(payload, 0, payload.Length);
        initial.Close();

        DelayedFileStream.ResetRead();
        DelayedFileStream reader = new DelayedFileStream("{{escapedPath}}", FileMode.Open);
        uint8[] destination = new uint8[payload.Length];
        Task<int32> read = reader.ReadAsync(destination, 0, destination.Length);
        int32 attempts = 0;
        while (!DelayedFileStream.ReadStarted && attempts < 2000)
        {
            Thread.Sleep(1);
            attempts++;
        }
        if (!DelayedFileStream.ReadStarted)
            return 1;
        read.RequestCancellation();
        try
        {
            int32 ignored = read.Result;
            return 2;
        }
        catch (OperationCanceledException)
        {
        }
        int32 readPosition = reader.Position;
        if (!read.IsCanceled || readPosition <= 0 || readPosition >= destination.Length)
            return 3;
        reader.Close();

        DelayedFileStream.ResetWrite();
        DelayedFileStream writer = new DelayedFileStream("{{escapedPath}}", FileMode.Create);
        Task write = writer.WriteAsync(payload, 0, payload.Length);
        attempts = 0;
        while (!DelayedFileStream.WriteStarted && attempts < 2000)
        {
            Thread.Sleep(1);
            attempts++;
        }
        if (!DelayedFileStream.WriteStarted)
            return 4;
        write.RequestCancellation();
        write.Wait();
        int32 writePosition = writer.Position;
        if (!write.IsCanceled || writePosition <= 0 || writePosition >= payload.Length)
            return 5;
        writer.Close();
        return 0;
    }
}
""";

        try
        {
            var execution = await BuildAndRunNativeAsync(source, "AsyncFileStreamCancellation");
            Assert.Equal(0, execution.ExitCode);
            Assert.Empty(execution.StandardError);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task TasksCanRequestCancellationAndAsyncBodiesCanObserveIt()
    {
        const string source = """
using System;
using System.Threading;

delegate bool CancellationNameProbe(bool IsCancellationRequested);

class Program
{
    private static object gate = new object();
    private static bool started;
    private static Task<int32>? childDelay;

    private static void ResetStarted()
    {
        lock (gate)
            started = false;
    }

    private static void MarkStarted()
    {
        lock (gate)
            started = true;
    }

    private static void WaitForStarted()
    {
        while (true)
        {
            lock (gate)
                if (started)
                    return;
            Thread.Sleep(1);
        }
    }

    public static async int32 CancelUsingQualifiedContext()
    {
        bool IsCancellationRequested = false;
        MarkStarted();
        while (!async.IsCancellationRequested)
            Thread.Sleep(1);
        async.ThrowIfCancellationRequested();
        return 1;
    }

    public static async int32 CancelUsingImplicitContext()
    {
        MarkStarted();
        while (!IsCancellationRequested)
            Thread.Sleep(1);
        ThrowIfCancellationRequested();
        return 2;
    }

    public static async int32 CancellationNamesRespectLexicalScope()
    {
        CancellationNameProbe probe = (bool IsCancellationRequested) => IsCancellationRequested;
        if (probe(false))
            return 15;
        {
            bool IsCancellationRequested = false;
            if (IsCancellationRequested)
                return 16;
        }
        MarkStarted();
        while (!IsCancellationRequested)
            Thread.Sleep(1);
        ThrowIfCancellationRequested();
        return 17;
    }

    public static async int32 DelayChild()
    {
        await Task.Delay(500);
        return 5;
    }

    public static async int32 CancelWhileAwaiting()
    {
        childDelay = DelayChild();
        MarkStarted();
        int32 value = await childDelay!;
        return 3;
    }

    public static async int32 IgnoreCancellation()
    {
        MarkStarted();
        Thread.Sleep(50);
        return 4;
    }

    public static async void CancelVoidOperation()
    {
        MarkStarted();
        while (!async.IsCancellationRequested)
            Thread.Sleep(1);
        async.ThrowIfCancellationRequested();
    }

    public static async int32 LocalShadowsImplicitContext()
    {
        bool IsCancellationRequested = false;
        return IsCancellationRequested ? 1 : 0;
    }

    public static int32 Main()
    {
        ResetStarted();
        Task<int32> qualified = CancelUsingQualifiedContext();
        WaitForStarted();
        qualified.RequestCancellation();
        if (!qualified.IsCancellationRequested)
            return 1;
        try
        {
            int32 value = qualified.Result;
            return 2;
        }
        catch (OperationCanceledException)
        {
            if (!qualified.IsCanceled)
                return 3;
        }

        ResetStarted();
        Task<int32> implicitContext = CancelUsingImplicitContext();
        WaitForStarted();
        implicitContext.RequestCancellation();
        try
        {
            int32 value = implicitContext.Result;
            return 4;
        }
        catch (OperationCanceledException)
        {
            if (!implicitContext.IsCanceled)
                return 5;
        }

        ResetStarted();
        Task<int32> scopedNames = CancellationNamesRespectLexicalScope();
        WaitForStarted();
        scopedNames.RequestCancellation();
        try
        {
            int32 value = scopedNames.Result;
            return 15;
        }
        catch (OperationCanceledException)
        {
            if (!scopedNames.IsCanceled)
                return 16;
        }

        ResetStarted();
        Task<int32> awaiting = CancelWhileAwaiting();
        WaitForStarted();
        awaiting.RequestCancellation();
        try
        {
            int32 value = awaiting.Result;
            return 17;
        }
        catch (OperationCanceledException)
        {
            if (!awaiting.IsCanceled)
                return 18;
        }
        Task<int32> completedChild = childDelay!;
        if (completedChild.Result != 5 || completedChild.IsCanceled)
            return 19;

        ResetStarted();
        Task<int32> cooperative = IgnoreCancellation();
        WaitForStarted();
        cooperative.RequestCancellation();
        if (cooperative.Result != 4 || !cooperative.IsCancellationRequested || cooperative.IsCanceled)
            return 20;

        ResetStarted();
        Task voidTask = CancelVoidOperation();
        WaitForStarted();
        voidTask.RequestCancellation();
        voidTask.Wait();
        if (!voidTask.IsCanceled || !voidTask.IsCancellationRequested)
            return 21;

        Task pendingDelay = Task.Delay(-1);
        pendingDelay.RequestCancellation();
        if (!pendingDelay.IsCompleted || !pendingDelay.IsCanceled || !pendingDelay.IsCancellationRequested)
            return 22;
        pendingDelay.Wait();

        Task scheduledDelay = Task.Delay(3600000);
        scheduledDelay.RequestCancellation();
        if (!scheduledDelay.IsCompleted || !scheduledDelay.IsCanceled)
            return 23;

        Task completed = Task.CompletedTask;
        completed.RequestCancellation();
        if (!completed.IsCompleted || completed.IsCanceled || completed.IsCancellationRequested)
            return 24;

        if (LocalShadowsImplicitContext().Result != 0)
            return 25;

        return 0;
    }
}
""";

        var execution = await BuildAndRunNativeAsync(source, "AsyncCancellation");
        Assert.Equal(0, execution.ExitCode);
        Assert.Empty(execution.StandardError);
        Assert.Empty(execution.StandardOutput);

        const string contextOutsideAsync = """
class Program
{
    public static int32 Main()
    {
        return async.IsCancellationRequested ? 1 : 0;
    }
}
""";
        var invalidContext = CompileWithRuntime(contextOutsideAsync, "async-context-outside-async.ls");
        Assert.False(invalidContext.Succeeded);
        Assert.Contains(invalidContext.Diagnostics, diagnostic => diagnostic.Code == "LS0105");
    }

    private static CompilationResult CompileWithRuntime(string source, string path)
    {
        var runtimeRoot = FindRuntimeRoot();
        var runtimeSources = Directory.EnumerateFiles(runtimeRoot, "*.ls", SearchOption.AllDirectories)
            .Select(sourcePath => (sourcePath, File.ReadAllText(sourcePath)));
        return LumaCompiler.Compile(runtimeSources.Append((path, source)), new CompilationOptions(UnsafeEnabled: true));
    }

    private static async Task<(string LlvmIr, int ExitCode, string StandardOutput, string StandardError)> BuildAndRunNativeAsync(
        string source, string outputName)
    {
        var workspace = Path.Combine(Path.GetTempPath(), "lumasharp-" + outputName + "-" + Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(workspace, "bin");
        var intermediateDirectory = Path.Combine(workspace, "obj");
        Directory.CreateDirectory(workspace);
        try
        {
            var sourcePath = Path.Combine(workspace, outputName + ".ls");
            await File.WriteAllTextAsync(sourcePath, source);
            var build = await LumaBuild.BuildAsync(new CompilerBuildRequest(
                [sourcePath],
                new CompilationOptions(UnsafeEnabled: true),
                outputDirectory,
                intermediateDirectory,
                outputName,
                FindRuntimeRoot()));
            Assert.True(build.Succeeded, string.Join(Environment.NewLine, build.Diagnostics));

            var llvmIr = await File.ReadAllTextAsync(Path.Combine(intermediateDirectory, outputName + ".ll"));
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo(build.OutputPaths.Single())
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var standardError = process.StandardError.ReadToEndAsync();
            var standardOutput = process.StandardOutput.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
                throw new TimeoutException($"Native async fixture '{outputName}' did not exit within 30 seconds.");
            }

            return (llvmIr, process.ExitCode, await standardOutput, await standardError);
        }
        finally
        {
            if (Directory.Exists(workspace))
                Directory.Delete(workspace, recursive: true);
        }
    }

    private static string FindRuntimeRoot()
    {
        var runtimeSource = Path.Combine("LumaSharp.Library", "System", "Object.ls");
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, runtimeSource);
            if (File.Exists(candidate))
                return Path.GetDirectoryName(Path.GetDirectoryName(candidate)!)!;
        }

        throw new FileNotFoundException("Could not locate the LumaSharp runtime library.");
    }
}
