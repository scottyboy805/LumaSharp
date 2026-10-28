using System.Diagnostics;
using LumaSharp.Compiler;

const int DefaultIterations = 100_000;
const int DefaultSamples = 5;
var iterations = ReadOption(args, "--iterations", DefaultIterations);
var samples = ReadOption(args, "--samples", DefaultSamples);
if (iterations < 1 || samples < 1)
{
    Console.Error.WriteLine("--iterations and --samples must be positive integers.");
    return 2;
}

var repositoryRoot = FindRepositoryRoot();
var fixturePath = Path.Combine(repositoryRoot, "Tests", "LumaSharp.Benchmarks", "Fixtures", "ListBenchmark.ls");
var runtimeSources = Directory.EnumerateFiles(Path.Combine(repositoryRoot, "LumaSharp.Library"), "*.ls", SearchOption.AllDirectories).ToArray();
var temporaryRoot = Path.Combine(Path.GetTempPath(), "lumasharp-list-benchmark-" + Guid.NewGuid().ToString("N"));
var generatedMain = Path.Combine(temporaryRoot, "BenchmarkMain.ls");
var executableDirectory = Path.Combine(temporaryRoot, "bin");
var intermediateDirectory = Path.Combine(temporaryRoot, "obj");
Directory.CreateDirectory(temporaryRoot);

try
{
    await File.WriteAllTextAsync(generatedMain,
        $"public class Program {{ public static int32 Main() {{ return ListBenchmark.Run({iterations}); }} }}\n");
    var build = await LumaBuild.BuildAsync(new CompilerBuildRequest(
        [fixturePath, generatedMain, .. runtimeSources],
        new CompilationOptions(UnsafeEnabled: true, Optimize: true),
        executableDirectory,
        intermediateDirectory,
        "ListBenchmark"));
    if (!build.Succeeded)
    {
        foreach (var diagnostic in build.Diagnostics)
            Console.Error.WriteLine(diagnostic);
        return 1;
    }

    var executable = build.OutputPaths.Single();
    var expectedChecksum = RunCSharp(iterations);
    var warmupResult = await RunNativeAsync(executable);
    if (warmupResult.ExitCode != expectedChecksum)
    {
        Console.Error.WriteLine($"LumaSharp result mismatch: expected {expectedChecksum}, got {warmupResult.ExitCode}. {warmupResult.Error}");
        return 1;
    }
    _ = RunCSharp(iterations);

    var csharpTimes = new List<double>(samples);
    var lumaTimes = new List<double>(samples);
    for (var sample = 0; sample < samples; sample++)
    {
        var csharpTimer = Stopwatch.StartNew();
        var checksum = RunCSharp(iterations);
        csharpTimer.Stop();
        if (checksum != expectedChecksum)
            throw new InvalidOperationException("C# benchmark returned an inconsistent checksum.");
        csharpTimes.Add(csharpTimer.Elapsed.TotalMilliseconds);

        var native = await RunNativeAsync(executable);
        if (native.ExitCode != expectedChecksum)
        {
            Console.Error.WriteLine($"LumaSharp result mismatch: expected {expectedChecksum}, got {native.ExitCode}. {native.Error}");
            return 1;
        }
        lumaTimes.Add(native.Elapsed);
    }

    var csharpMedian = Median(csharpTimes);
    var lumaMedian = Median(lumaTimes);
    Console.WriteLine($"List<T> workload: {iterations:N0} iterations, {samples} measured samples, checksum {expectedChecksum}");
    Console.WriteLine($"C# List<int> median:       {csharpMedian,10:F3} ms");
    Console.WriteLine($"LumaSharp List<int32> median: {lumaMedian,10:F3} ms (includes native process startup)");
    Console.WriteLine($"C# / LumaSharp:            {csharpMedian / lumaMedian,10:F2}x");
    Console.WriteLine("LumaSharp is compiled with LLVM optimization enabled. Timings are local measurements, not a stable benchmark result.");
    return 0;
}
finally
{
    try { Directory.Delete(temporaryRoot, recursive: true); }
    catch (IOException) { }
    catch (UnauthorizedAccessException) { }
}

static int ReadOption(string[] arguments, string name, int defaultValue)
{
    var index = Array.IndexOf(arguments, name);
    if (index < 0) return defaultValue;
    if (index + 1 >= arguments.Length || !int.TryParse(arguments[index + 1], out var value))
        throw new ArgumentException($"{name} requires an integer value.");
    return value;
}

static string FindRepositoryRoot()
{
    for (var directory = new DirectoryInfo(Environment.CurrentDirectory); directory is not null; directory = directory.Parent)
        if (File.Exists(Path.Combine(directory.FullName, "LumaSharp.slnx")) && Directory.Exists(Path.Combine(directory.FullName, "LumaSharp.Library")))
            return directory.FullName;
    throw new DirectoryNotFoundException("Run the benchmark from inside the LumaSharp repository.");
}

static int RunCSharp(int iterations)
{
    var defaultList = new List<int>();
    var list = new List<int>(4);
    var checksum = defaultList.Capacity;
    for (var iteration = 0; iteration < iterations; iteration++)
    {
        list.Clear();
        list.Add(iteration);
        list.Add(iteration + 1);
        list.Add(iteration + 2);

        checksum = (checksum + list.Count + list.Capacity) % 127;
        checksum = (checksum + list[0] + list[1] + list[2]) % 127;
        list[1] = iteration + 3;
        checksum = (checksum + list.IndexOf(iteration + 3)) % 127;
        if (list.Contains(iteration + 2))
            checksum = (checksum + 1) % 127;

        list.Insert(1, iteration + 4);
        checksum = (checksum + list[1]) % 127;
        list.RemoveAt(1);
        if (list.Remove(iteration + 2))
            checksum = (checksum + 1) % 127;

        checksum = (checksum + list.ToArray().Length) % 127;
        foreach (var item in list)
            checksum = (checksum + item) % 127;
    }

    list.Clear();
    GC.KeepAlive(defaultList);
    GC.KeepAlive(list);
    return checksum;
}

static async Task<(int ExitCode, double Elapsed, string Error)> RunNativeAsync(string executable)
{
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
    var timer = Stopwatch.StartNew();
    process.Start();
    var standardError = process.StandardError.ReadToEndAsync();
    var standardOutput = process.StandardOutput.ReadToEndAsync();
    await process.WaitForExitAsync();
    timer.Stop();
    _ = await standardOutput;
    return (process.ExitCode, timer.Elapsed.TotalMilliseconds, await standardError);
}

static double Median(List<double> values)
{
    var ordered = values.Order().ToArray();
    var middle = ordered.Length / 2;
    return ordered.Length % 2 == 0 ? (ordered[middle - 1] + ordered[middle]) / 2 : ordered[middle];
}
