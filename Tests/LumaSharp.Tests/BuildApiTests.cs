using System.Diagnostics;
using LumaSharp.Compiler;
using Xunit;

namespace LumaSharp.Tests;

public sealed class BuildApiTests
{
    [Fact]
    public async Task LibraryBuildApiProducesVerifiedIrWithoutUsingCli()
    {
        using var workspace = new TemporaryWorkspace();
        var source = workspace.Write("Program.ls", "class Program { public static int32 Main() { return 42; } }");
        var output = Path.Combine(workspace.Root, "bin");
        var intermediate = Path.Combine(workspace.Root, "obj");

        var result = await LumaBuild.BuildAsync(new CompilerBuildRequest(
            [source],
            new CompilationOptions(EmitLlvmIrOnly: true),
            output,
            intermediate,
            "Program"));

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var ir = Assert.Single(result.OutputPaths);
        Assert.Equal(Path.Combine(intermediate, "Program.ll"), ir);
        Assert.Contains("define i32 @main()", File.ReadAllText(ir));
        Assert.Empty(Directory.EnumerateFiles(output));
    }

    [Fact]
    public async Task LibraryBuildApiProducesStaticArchiveWithoutExecutableEntryPoint()
    {
        // Native librarian processes run outside the managed test host. Keep this
        // integration fixture under the project tree so restricted test runners
        // can grant the child process the same workspace access as the compiler.
        using var workspace = new TemporaryWorkspace(AppContext.BaseDirectory);
        var source = workspace.Write("Utilities.ls", "public class Utilities { public static int32 Twice(int32 value) { return value * 2; } }");
        var output = Path.Combine(workspace.Root, "lib");
        var intermediate = Path.Combine(workspace.Root, "obj");

        var result = await LumaBuild.BuildAsync(new CompilerBuildRequest(
            [source],
            new CompilationOptions(StaticLibrary: true),
            output,
            intermediate,
            "Utilities"));

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var archive = Assert.Single(result.OutputPaths);
        Assert.Equal(Path.Combine(output, "Utilities" + (OperatingSystem.IsWindows() ? ".lib" : ".a")), archive);
        Assert.True(File.Exists(archive));
        Assert.Equal("!<arch>\n", System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(archive).Take(8).ToArray()));
        var ir = File.ReadAllText(Path.Combine(intermediate, "Utilities.ll"));
        Assert.DoesNotContain("define i32 @main()", ir);
        Assert.Contains("define internal void @luma_throw_overflow()", ir);
        Assert.Contains("define internal void @luma_System_Object_ctor_0(", ir);
        Assert.Empty(Directory.EnumerateFiles(intermediate, "*.tmp.lib"));
    }

    [Fact]
    public async Task LibraryBuildApiCompilesMultipleFilesIntoRunnableNativeExecutable()
    {
        using var workspace = new TemporaryWorkspace(AppContext.BaseDirectory);
        var firstSource = workspace.Write("Maths.ls", "public class Maths { public static int32 Answer() { return 42; } }");
        var entrySource = workspace.Write("Program.ls", "public class Program { public static int32 Main() { return Maths.Answer(); } }");
        var output = Path.Combine(workspace.Root, "bin");
        var intermediate = Path.Combine(workspace.Root, "obj");

        var result = await LumaBuild.BuildAsync(new CompilerBuildRequest(
            [firstSource, entrySource],
            new CompilationOptions(),
            output,
            intermediate,
            "Smoke"));

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var executable = Assert.Single(result.OutputPaths);
        Assert.True(File.Exists(executable));
        Assert.Equal(Path.Combine(output, "Smoke" + (OperatingSystem.IsWindows() ? ".exe" : "")), executable);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(executable)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        var stderr = process.StandardError.ReadToEndAsync();
        var stdout = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        Assert.Equal(42, process.ExitCode);
        Assert.Empty(await stderr);
        Assert.Empty(await stdout);
    }

    [Fact]
    public async Task RuntimeConsoleWritesHelloWorldToStandardOutput()
    {
        using var workspace = new TemporaryWorkspace(AppContext.BaseDirectory);
        var runtimeRoot = FindRuntimeRoot();
        var runtimeSources = Directory.EnumerateFiles(runtimeRoot, "*.ls", SearchOption.AllDirectories);
        var program = workspace.Write("Program.ls", "using System; public class Program { public static int32 Main() { Console.WriteLine(\"Hello, LumaSharp! — 世界 🌍\"); return 0; } }");
        var output = Path.Combine(workspace.Root, "bin");
        var intermediate = Path.Combine(workspace.Root, "obj");

        var result = await LumaBuild.BuildAsync(new CompilerBuildRequest(
            runtimeSources.Append(program).ToArray(),
            new CompilationOptions(UnsafeEnabled: true),
            output,
            intermediate,
            "ConsoleSmoke"));

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(result.OutputPaths.Single())
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        var stderr = process.StandardError.ReadToEndAsync();
        var stdout = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        Assert.Equal(0, process.ExitCode);
        Assert.Equal("Hello, LumaSharp! — 世界 🌍" + Environment.NewLine, await stdout);
        Assert.Empty(await stderr);
    }

    [Fact]
    public async Task RuntimeEnvironmentReportsTheRunningExecutableAndHost()
    {
        using var workspace = new TemporaryWorkspace(AppContext.BaseDirectory);
        var program = workspace.Write("Program.ls", "using System; public class Program { public static int32 Main() { Console.WriteLine(Environment.ExecutablePath); Console.WriteLine(Environment.ExecutableDirectory); Console.WriteLine(Environment.ExecutableName); Console.WriteLine(Environment.ProcessorCount.ToString()); Console.WriteLine(Environment.IsLittleEndian.ToString()); Console.WriteLine(((int32)Environment.Platform).ToString()); Console.WriteLine(((int32)Environment.Architecture).ToString()); return 0; } }");
        var output = Path.Combine(workspace.Root, "bin");
        var intermediate = Path.Combine(workspace.Root, "obj");

        var result = await LumaBuild.BuildAsync(new CompilerBuildRequest(
            [program],
            new CompilationOptions(),
            output,
            intermediate,
            "EnvironmentSmoke"));

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(result.OutputPaths.Single())
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        var stderr = process.StandardError.ReadToEndAsync();
        var stdout = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        Assert.Equal(0, process.ExitCode);
        Assert.Empty(await stderr);
        var values = (await stdout).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(7, values.Length);
        Assert.Equal(Path.GetFullPath(result.OutputPaths.Single()), values[0], OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        Assert.Equal(Path.GetDirectoryName(Path.GetFullPath(result.OutputPaths.Single())), values[1], OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        Assert.Equal(Path.GetFileName(result.OutputPaths.Single()), values[2]);
        Assert.True(int.TryParse(values[3], out var processorCount) && processorCount > 0);
        Assert.Equal(BitConverter.IsLittleEndian ? "True" : "False", values[4]);
        Assert.Equal(OperatingSystem.IsWindows() ? "1" : OperatingSystem.IsLinux() ? "2" : OperatingSystem.IsMacOS() ? "3" : "0", values[5]);
        Assert.Equal(System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture switch
        {
            System.Runtime.InteropServices.Architecture.X86 => "1",
            System.Runtime.InteropServices.Architecture.X64 => "2",
            System.Runtime.InteropServices.Architecture.Arm => "3",
            System.Runtime.InteropServices.Architecture.Arm64 => "4",
            _ => "0"
        }, values[6]);
    }

    [Fact]
    public async Task RuntimeStopWatchTracksAKnownNativeDelay()
    {
        using var workspace = new TemporaryWorkspace(AppContext.BaseDirectory);
        var delayDeclaration = OperatingSystem.IsWindows()
            ? "public static extern void Sleep(uint32 milliseconds);"
            : "public static extern int32 usleep(uint32 microseconds);";
        var delayCall = OperatingSystem.IsWindows()
            ? "NativeDelay.Sleep(400);"
            : "NativeDelay.usleep(400000);";
        var program = workspace.Write("Program.ls", $$"""
            using System;
            using System.Diagnostics;

            internal static unsafe class NativeDelay
            {
                {{delayDeclaration}}
            }

            public class Program
            {
                public static int32 Main()
                {
                    StopWatch watch = StopWatch.StartNew();
                    unsafe
                    {
                        {{delayCall}}
                    }
                    watch.Stop();
                    Console.WriteLine(watch.Elapsed.Ticks.ToString());
                    return 0;
                }
            }
            """);
        var output = Path.Combine(workspace.Root, "bin");
        var intermediate = Path.Combine(workspace.Root, "obj");

        var build = await LumaBuild.BuildAsync(new CompilerBuildRequest(
            [program],
            new CompilationOptions(UnsafeEnabled: true),
            output,
            intermediate,
            "StopWatchAccuracy",
            FindRuntimeRoot()));

        Assert.True(build.Succeeded, string.Join(Environment.NewLine, build.Diagnostics));
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(build.OutputPaths.Single())
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        var stderr = process.StandardError.ReadToEndAsync();
        var stdout = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        Assert.Equal(0, process.ExitCode);
        Assert.Empty(await stderr);
        Assert.True(long.TryParse((await stdout).Trim(), out var elapsedTicks));
        Assert.InRange(elapsedTicks, 3_500_000, 6_000_000);
    }

    private static string FindRuntimeRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "LumaSharp.Library");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Could not locate the LumaSharp runtime sources.");
    }

    [Fact]
    public async Task SeparatelyBuiltStaticLibrariesLinkTogetherOnWindows()
    {
        if (!OperatingSystem.IsWindows()) return;

        using var workspace = new TemporaryWorkspace(AppContext.BaseDirectory);
        var firstSource = workspace.Write("First.ls", "public class First { public static int32 Answer() { return 40; } }");
        var secondSource = workspace.Write("Second.ls", "public class Second { public static int32 Answer() { return 2; } }");
        var output = Path.Combine(workspace.Root, "lib");
        var firstIntermediate = Path.Combine(workspace.Root, "obj-first");
        var secondIntermediate = Path.Combine(workspace.Root, "obj-second");

        var first = await BuildStaticLibrary(firstSource, "First", firstIntermediate);
        var second = await BuildStaticLibrary(secondSource, "Second", secondIntermediate);
        Assert.True(first.Succeeded, string.Join(Environment.NewLine, first.Diagnostics));
        Assert.True(second.Succeeded, string.Join(Environment.NewLine, second.Diagnostics));

        var llvmBin = FindWindowsLlvmBin();
        Assert.NotNull(llvmBin);
        var cSource = workspace.Write("main.c", "extern int luma_First_Answer_0(void); extern int luma_Second_Answer_0(void); int main(void) { return luma_First_Answer_0() + luma_Second_Answer_0() == 42 ? 0 : 1; }");
        var nativeObject = Path.Combine(workspace.Root, "main.obj");
        var executable = Path.Combine(workspace.Root, "combined.exe");

        var cCompile = await RunNativeTool(Path.Combine(llvmBin!, "clang-cl.exe"), ["/nologo", "/c", cSource, $"/Fo{nativeObject}"]);
        Assert.True(cCompile.ExitCode == 0, cCompile.Output);
        var link = await RunNativeTool(Path.Combine(llvmBin!, "lld-link.exe"),
            ["/nologo", "/entry:main", "/subsystem:console", "/nodefaultlib", $"/out:{executable}", nativeObject,
                first.OutputPaths.Single(), second.OutputPaths.Single()]);
        Assert.True(link.ExitCode == 0, link.Output);

        using var process = Process.Start(new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true });
        Assert.NotNull(process);
        await process.WaitForExitAsync();
        Assert.Equal(0, process.ExitCode);

        async Task<CompilerBuildResult> BuildStaticLibrary(string source, string name, string intermediate) =>
            await LumaBuild.BuildAsync(new CompilerBuildRequest([source], new CompilationOptions(StaticLibrary: true),
                output, intermediate, name));
    }

    private static string? FindWindowsLlvmBin()
    {
        var roots = new[] { Environment.GetEnvironmentVariable("LLVM_HOME"), Environment.GetEnvironmentVariable("LLVM_ROOT"),
            Environment.GetEnvironmentVariable("ProgramW6432"), Environment.GetEnvironmentVariable("ProgramFiles") }
            .Where(root => !string.IsNullOrWhiteSpace(root)).Select(root => root!.Trim('"'));
        foreach (var root in roots)
        {
            var candidate = Path.GetFileName(root).Equals("bin", StringComparison.OrdinalIgnoreCase)
                ? root : Path.Combine(root, "bin");
            if (File.Exists(Path.Combine(candidate, "clang-cl.exe")) && File.Exists(Path.Combine(candidate, "lld-link.exe"))) return candidate;
            var nested = Path.Combine(root, "LLVM", "bin");
            if (File.Exists(Path.Combine(nested, "clang-cl.exe")) && File.Exists(Path.Combine(nested, "lld-link.exe"))) return nested;
        }
        return null;
    }

    private static async Task<(int ExitCode, string Output)> RunNativeTool(string executable, IReadOnlyList<string> arguments)
    {
        using var process = new Process { StartInfo = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        }};
        foreach (var argument in arguments) process.StartInfo.ArgumentList.Add(argument);
        process.Start();
        var stderr = process.StandardError.ReadToEndAsync();
        var stdout = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, (await stderr) + (await stdout));
    }

    [Fact]
    public async Task LibraryBuildApiReturnsStructuredInputDiagnostics()
    {
        using var workspace = new TemporaryWorkspace();
        var result = await LumaBuild.BuildAsync(new CompilerBuildRequest(
            [Path.Combine(workspace.Root, "missing.ls")],
            new CompilationOptions(EmitLlvmIrOnly: true),
            Path.Combine(workspace.Root, "bin"),
            Path.Combine(workspace.Root, "obj"),
            "Program"));

        Assert.False(result.Succeeded);
        Assert.Empty(result.OutputPaths);
        Assert.Contains(result.Diagnostics, d => d.Code == "LMC1005" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task LibraryBuildApiStopsOnCancellationWithoutPublishingOutput()
    {
        using var workspace = new TemporaryWorkspace();
        var source = workspace.Write("Program.ls", "class Program { public static void Main() {} }");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await LumaBuild.BuildAsync(new CompilerBuildRequest(
            [source],
            new CompilationOptions(EmitLlvmIrOnly: true),
            Path.Combine(workspace.Root, "bin"),
            Path.Combine(workspace.Root, "obj"),
            "Program"), cancellation.Token);

        Assert.False(result.Succeeded);
        Assert.True(result.Cancelled);
        Assert.Empty(result.OutputPaths);
        Assert.Contains(result.Diagnostics, d => d.Code == "LMC1007");
        Assert.False(Directory.Exists(Path.Combine(workspace.Root, "bin")));
    }

    [Fact]
    public async Task SourceErrorsDoNotCreateBuildDirectoriesOrOutputs()
    {
        using var workspace = new TemporaryWorkspace();
        var source = workspace.Write("Program.ls", "class Program { public static int32 Main() { return false; } }");
        var output = Path.Combine(workspace.Root, "bin");
        var intermediate = Path.Combine(workspace.Root, "obj");

        var result = await LumaBuild.BuildAsync(new CompilerBuildRequest(
            [source],
            new CompilationOptions(EmitLlvmIrOnly: true),
            output,
            intermediate,
            "Program"));

        Assert.False(result.Succeeded);
        Assert.Empty(result.OutputPaths);
        Assert.Contains(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.False(Directory.Exists(output));
        Assert.False(Directory.Exists(intermediate));
    }

    [Fact]
    public async Task InvalidTargetIsRejectedBeforeParsingOrCreatingBuildDirectories()
    {
        using var workspace = new TemporaryWorkspace();
        var source = workspace.Write("Program.ls", "class Program { public static int32 Main() { return false; } }");
        var output = Path.Combine(workspace.Root, "bin");
        var intermediate = Path.Combine(workspace.Root, "obj");

        var result = await LumaBuild.BuildAsync(new CompilerBuildRequest(
            [source],
            new CompilationOptions(EmitLlvmIrOnly: true, TargetTriple: "lumasharp-unknown-architecture-none"),
            output,
            intermediate,
            "Program"));

        Assert.False(result.Succeeded);
        Assert.Empty(result.OutputPaths);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LMC2001" &&
            diagnostic.Message.Contains("architecture", StringComparison.OrdinalIgnoreCase));
        Assert.False(Directory.Exists(output));
        Assert.False(Directory.Exists(intermediate));
    }

    [Theory]
    [InlineData("link", "x86_64-pc-windows-msvc", "x86_64-pc-windows-msvc", false)]
    [InlineData("clang-cl", "x86_64-pc-windows-msvc", "x86_64-pc-windows-msvc", false)]
    [InlineData("link", "i686-pc-windows-msvc", "x86_64-pc-windows-msvc", true)]
    [InlineData("clang-cl", "i686-pc-windows-msvc", "x86_64-pc-windows-msvc", true)]
    [InlineData("clang", "i686-pc-windows-gnu", "x86_64-pc-windows-msvc", false)]
    [InlineData("link", "i686-pc-windows-msvc", null, true)]
    public void HostOnlyLinkersRejectTargetsTheyCannotGuarantee(string linker, string requestedTarget, string? hostTarget, bool shouldReject)
    {
        var error = LumaBuild.ValidateHostOnlyLinkerTarget(linker, requestedTarget, hostTarget);
        Assert.Equal(shouldReject, error is not null);
        if (shouldReject) Assert.Contains(requestedTarget, error!, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        public string Root { get; }

        public TemporaryWorkspace(string? parent = null)
        {
            Root = Path.Combine(parent ?? Path.GetTempPath(), "luma-build-api-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }
        public string Write(string name, string text)
        {
            var path = Path.Combine(Root, name);
            File.WriteAllText(path, text);
            return path;
        }
        public void Dispose() { if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true); }
    }
}
