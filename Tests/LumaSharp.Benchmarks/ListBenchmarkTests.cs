using System.Diagnostics;
using LumaSharp.Compiler;
using Xunit;
using Xunit.Abstractions;

namespace LumaSharp.Benchmarks;

public sealed class ListBenchmarkTests(ITestOutputHelper output)
{
    private const int Iterations = 250_000;
    private const int Samples = 7;

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
                $"using System; public class Program {{ public static int32 Main() {{ int32 checksum = ListBenchmark.Run({Iterations}); Console.WriteLine(checksum.ToString()); Console.WriteLine(ListBenchmark.ElapsedTicks.ToString()); return 0; }} }}\n");
            var build = await LumaBuild.BuildAsync(new CompilerBuildRequest(
                [fixturePath, generatedMain, .. runtimeSources],
                new CompilationOptions(UnsafeEnabled: true, Optimize: true),
                executableDirectory,
                intermediateDirectory,
                "ListBenchmark"));

            Assert.True(build.Succeeded, string.Join(Environment.NewLine, build.Diagnostics));
            var executable = Assert.Single(build.OutputPaths);
            var expectedChecksum = CSharpListBenchmark.Run(Iterations).Checksum;
            var warmup = await RunNativeAsync(executable);
            Assert.Equal(expectedChecksum, warmup.Checksum);
            _ = CSharpListBenchmark.Run(Iterations);

            var csharpTimes = new List<double>(Samples);
            var lumaTimes = new List<double>(Samples);
            for (var sample = 0; sample < Samples; sample++)
            {
                var csharp = CSharpListBenchmark.Run(Iterations);
                Assert.Equal(expectedChecksum, csharp.Checksum);
                csharpTimes.Add(csharp.ElapsedMilliseconds);

                var native = await RunNativeAsync(executable);
                Assert.Equal(0, native.ExitCode);
                Assert.True(string.IsNullOrWhiteSpace(native.Error), native.Error);
                Assert.Equal(expectedChecksum, native.Checksum);
                var lumaMilliseconds = native.ElapsedTicks / 10_000.0;
                lumaTimes.Add(lumaMilliseconds);
                output.WriteLine($"Sample {sample + 1}: C# {csharp.ElapsedMilliseconds:F3} ms; LumaSharp {lumaMilliseconds:F3} ms");
            }

            var csharpMedian = Median(csharpTimes);
            var lumaMedian = Median(lumaTimes);
            output.WriteLine($"List<T>: {Iterations:N0} iterations; {Samples} samples; checksum {expectedChecksum}");
            output.WriteLine($"C# List<int> internal median: {csharpMedian:F3} ms ({csharpTimes.Min():F3}–{csharpTimes.Max():F3} ms)");
            output.WriteLine($"LumaSharp List<int32> internal median: {lumaMedian:F3} ms ({lumaTimes.Min():F3}–{lumaTimes.Max():F3} ms)");
            output.WriteLine($"C# / LumaSharp: {csharpMedian / lumaMedian:F2}x (values above 1 mean LumaSharp is faster)");
            output.WriteLine("LumaSharp's reported duration comes from StopWatch inside the generated executable; build time and process startup are excluded.");
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

    private static async Task<(int ExitCode, int Checksum, long ElapsedTicks, string Error)> RunNativeAsync(string executable)
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
        process.Start();
        var standardError = process.StandardError.ReadToEndAsync();
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        var values = (await standardOutput).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        if (values.Length != 2 || !int.TryParse(values[0], out var checksum) || !long.TryParse(values[1], out var elapsedTicks))
            return (process.ExitCode, 0, 0, $"Unexpected benchmark output: '{string.Join(" | ", values)}'. {await standardError}");
        return (process.ExitCode, checksum, elapsedTicks, await standardError);
    }

    private static double Median(List<double> values)
    {
        var ordered = values.Order().ToArray();
        var middle = ordered.Length / 2;
        return ordered.Length % 2 == 0 ? (ordered[middle - 1] + ordered[middle]) / 2 : ordered[middle];
    }
}
