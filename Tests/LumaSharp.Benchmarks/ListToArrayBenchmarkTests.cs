using System.Diagnostics;
using LumaSharp.Compiler;
using Xunit;
using Xunit.Abstractions;

namespace LumaSharp.Benchmarks;

public sealed class ListToArrayBenchmarkTests(ITestOutputHelper output)
{
    private const int ItemCount = 3_072;
    private const int Copies = 5_000;
    private const int Samples = 5;

    [Fact]
    public async Task CompareListToArrayWithCSharpListToArrayUnderHighStress()
    {
        var repositoryRoot = FindRepositoryRoot();
        var fixturePath = Path.Combine(repositoryRoot, "Tests", "LumaSharp.Benchmarks", "Fixtures", "ListToArrayBenchmark.ls");
        var runtimeSources = Directory.EnumerateFiles(Path.Combine(repositoryRoot, "LumaSharp.Library"), "*.ls", SearchOption.AllDirectories).ToArray();
        var temporaryRoot = Path.Combine(Path.GetTempPath(), "lumasharp-list-to-array-benchmark-" + Guid.NewGuid().ToString("N"));
        var generatedMain = Path.Combine(temporaryRoot, "BenchmarkMain.ls");
        Directory.CreateDirectory(temporaryRoot);

        try
        {
            await File.WriteAllTextAsync(generatedMain,
                $"using System; public class Program {{ public static int32 Main() {{ int32 checksum = ListToArrayBenchmark.Run({ItemCount}, {Copies}); Console.WriteLine(checksum.ToString()); Console.WriteLine(ListToArrayBenchmark.ElapsedTicks.ToString()); return 0; }} }}\n");
            var build = await LumaBuild.BuildAsync(new CompilerBuildRequest(
                [fixturePath, generatedMain, .. runtimeSources],
                new CompilationOptions(UnsafeEnabled: true, Optimize: true),
                Path.Combine(temporaryRoot, "bin"),
                Path.Combine(temporaryRoot, "obj"),
                "ListToArrayBenchmark"));

            Assert.True(build.Succeeded, string.Join(Environment.NewLine, build.Diagnostics));
            var executable = Assert.Single(build.OutputPaths);
            var csharpWarmup = CSharpListToArrayBenchmark.Run(ItemCount, Copies);
            var lumaWarmup = await RunNativeAsync(executable);
            Assert.Equal(0, lumaWarmup.ExitCode);
            Assert.True(string.IsNullOrWhiteSpace(lumaWarmup.Error), lumaWarmup.Error);
            Assert.Equal(csharpWarmup.Checksum, lumaWarmup.Checksum);

            var csharpTimes = new List<double>(Samples);
            var lumaTimes = new List<double>(Samples);
            for (var sample = 0; sample < Samples; sample++)
            {
                var csharp = CSharpListToArrayBenchmark.Run(ItemCount, Copies);
                Assert.Equal(csharpWarmup.Checksum, csharp.Checksum);
                csharpTimes.Add(csharp.ElapsedMilliseconds);

                var native = await RunNativeAsync(executable);
                Assert.Equal(0, native.ExitCode);
                Assert.True(string.IsNullOrWhiteSpace(native.Error), native.Error);
                Assert.Equal(csharpWarmup.Checksum, native.Checksum);
                var lumaMilliseconds = native.ElapsedTicks / 10_000.0;
                lumaTimes.Add(lumaMilliseconds);
                output.WriteLine($"Sample {sample + 1}: C# {csharp.ElapsedMilliseconds:F3} ms; LumaSharp {lumaMilliseconds:F3} ms");
            }

            var csharpMedian = Median(csharpTimes);
            var lumaMedian = Median(lumaTimes);
            var copiedMiB = (double)ItemCount * sizeof(int) * Copies / (1024 * 1024);
            output.WriteLine($"List<int>.ToArray: {ItemCount:N0} elements × {Copies:N0} copies per sample ({copiedMiB:F1} MiB copied); {Samples} samples; checksum {csharpWarmup.Checksum}");
            output.WriteLine($"C# List<int>.ToArray median: {csharpMedian:F3} ms ({csharpTimes.Min():F3}–{csharpTimes.Max():F3} ms), {copiedMiB / (csharpMedian / 1_000):F1} MiB/s");
            output.WriteLine($"LumaSharp List<int32>.ToArray median: {lumaMedian:F3} ms ({lumaTimes.Min():F3}–{lumaTimes.Max():F3} ms), {copiedMiB / (lumaMedian / 1_000):F1} MiB/s");
            output.WriteLine($"C# / LumaSharp: {csharpMedian / lumaMedian:F2}x (above 1 means LumaSharp is faster)");
            output.WriteLine("Timers cover only repeated ToArray calls and checksum reads; build time, process startup, list construction, and one full correctness scan are excluded.");
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
