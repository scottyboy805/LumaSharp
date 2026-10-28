using System.Diagnostics;
using LumaSharp.Compiler;
using Xunit;
using Xunit.Abstractions;

namespace LumaSharp.Benchmarks;

public sealed class ListBenchmarkTests(ITestOutputHelper output)
{
    private const int Iterations = 100_000;
    private const int Samples = 5;

    [Fact]
    public async Task CompareRuntimeListWithCSharpList()
    {
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
                $"public class Program {{ public static int32 Main() {{ return ListBenchmark.Run({Iterations}); }} }}\n");
            var build = await LumaBuild.BuildAsync(new CompilerBuildRequest(
                [fixturePath, generatedMain, .. runtimeSources],
                new CompilationOptions(UnsafeEnabled: true, Optimize: true),
                executableDirectory,
                intermediateDirectory,
                "ListBenchmark"));

            Assert.True(build.Succeeded, string.Join(Environment.NewLine, build.Diagnostics));
            var executable = Assert.Single(build.OutputPaths);
            var expectedChecksum = CSharpListBenchmark.Run(Iterations);
            var warmup = await RunNativeAsync(executable);
            Assert.Equal(expectedChecksum, warmup.ExitCode);
            _ = CSharpListBenchmark.Run(Iterations);

            var csharpTimes = new List<double>(Samples);
            var lumaTimes = new List<double>(Samples);
            for (var sample = 0; sample < Samples; sample++)
            {
                var csharpTimer = Stopwatch.StartNew();
                Assert.Equal(expectedChecksum, CSharpListBenchmark.Run(Iterations));
                csharpTimer.Stop();
                csharpTimes.Add(csharpTimer.Elapsed.TotalMilliseconds);

                var native = await RunNativeAsync(executable);
                Assert.Equal(expectedChecksum, native.ExitCode);
                Assert.True(string.IsNullOrWhiteSpace(native.Error), native.Error);
                lumaTimes.Add(native.ElapsedMilliseconds);
            }

            var csharpMedian = Median(csharpTimes);
            var lumaMedian = Median(lumaTimes);
            output.WriteLine($"List<T>: {Iterations:N0} iterations; {Samples} samples; checksum {expectedChecksum}");
            output.WriteLine($"C# List<int> median: {csharpMedian:F3} ms");
            output.WriteLine($"LumaSharp List<int32> median: {lumaMedian:F3} ms (includes native process startup)");
            output.WriteLine($"C# / LumaSharp: {csharpMedian / lumaMedian:F2}x");
        }
        finally
        {
            try { Directory.Delete(temporaryRoot, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(Environment.CurrentDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "LumaSharp.slnx")) && Directory.Exists(Path.Combine(directory.FullName, "LumaSharp.Library")))
                return directory.FullName;
        throw new DirectoryNotFoundException("Run the benchmark from inside the LumaSharp repository.");
    }

    private static async Task<(int ExitCode, double ElapsedMilliseconds, string Error)> RunNativeAsync(string executable)
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
        _ = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        timer.Stop();
        return (process.ExitCode, timer.Elapsed.TotalMilliseconds, await standardError);
    }

    private static double Median(List<double> values)
    {
        var ordered = values.Order().ToArray();
        var middle = ordered.Length / 2;
        return ordered.Length % 2 == 0 ? (ordered[middle - 1] + ordered[middle]) / 2 : ordered[middle];
    }
}
