using System.Diagnostics;
using LumaSharp.Compiler;
using Xunit;
using Xunit.Abstractions;

namespace LumaSharp.Benchmarks;

public sealed class CompilerPerformanceBenchmarkTests(ITestOutputHelper output)
{
    private const int Samples = 3;

    [Fact]
    public async Task ProfileFullRuntimeLibraryBuildByCompilerPhase()
    {
        var repositoryRoot = FindRepositoryRoot();
        var fixture = Path.Combine(repositoryRoot, "Tests", "LumaSharp.Benchmarks", "Fixtures", "CompilerBuildBenchmark.ls");
        var runtimePath = Path.Combine(repositoryRoot, "LumaSharp.Library");
        var temporaryRoot = Path.Combine(Path.GetTempPath(), "lumasharp-compiler-profile-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryRoot);

        try
        {
            foreach (var optimize in new[] { false, true })
            {
                var options = new CompilationOptions(Optimize: optimize);
                var request = new CompilerBuildRequest(
                    [fixture], options,
                    Path.Combine(temporaryRoot, optimize ? "release-bin" : "debug-bin"),
                    Path.Combine(temporaryRoot, optimize ? "release-obj" : "debug-obj"),
                    optimize ? "CompilerProfileRelease" : "CompilerProfileDebug",
                    runtimePath);

                var warmup = await RunSample(request);
                Assert.True(warmup.Result.Succeeded, string.Join(Environment.NewLine, warmup.Result.Diagnostics));

                var timings = new List<double>(Samples);
                var phaseRuns = new List<IReadOnlyDictionary<string, CompilationPhaseSample>>(Samples);
                for (var sample = 0; sample < Samples; sample++)
                {
                    var measured = await RunSample(request);
                    Assert.True(measured.Result.Succeeded, string.Join(Environment.NewLine, measured.Result.Diagnostics));
                    timings.Add(measured.ElapsedMilliseconds);
                    phaseRuns.Add(measured.Phases);
                }

                output.WriteLine($"{(optimize ? "Optimized" : "Debug")} build: {Median(timings):F1} ms median total ({timings.Min():F1}–{timings.Max():F1} ms), {Samples} measured samples after one warmup; LLVM IR {warmup.IrBytes:N0} bytes.");
                foreach (var phase in phaseRuns.SelectMany(run => run.Keys).Distinct()
                             .Where(name => !name.StartsWith("LLVM method: ", StringComparison.Ordinal))
                             .OrderBy(name => name, StringComparer.Ordinal))
                {
                    var medians = phaseRuns.Select(run => run.TryGetValue(phase, out var sample) ? sample.Elapsed.TotalMilliseconds : 0d).ToArray();
                    var allocated = phaseRuns.Select(run => run.TryGetValue(phase, out var sample) ? sample.AllocatedBytes : 0L).ToArray();
                    var collections = phaseRuns.Select(run => run.TryGetValue(phase, out var sample)
                        ? sample.Gen0Collections + sample.Gen1Collections + sample.Gen2Collections : 0).ToArray();
                    var allocationText = allocated.Any(value => value < 0) ? "n/a (async thread switch)" : $"{Median(allocated):N0} B";
                    output.WriteLine($"  {phase}: {Median(medians):F1} ms; {allocationText}; {Median(collections):F0} GCs");
                }
                var methodSamples = phaseRuns.SelectMany(run => run)
                    .Where(pair => pair.Key.StartsWith("LLVM method: ", StringComparison.Ordinal))
                    .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(group => (Name: group.Key, Allocations: group.Select(pair => pair.Value.AllocatedBytes).ToArray(),
                        Times: group.Select(pair => pair.Value.Elapsed.TotalMilliseconds).ToArray()))
                    .OrderByDescending(group => group.Allocations.Any(value => value < 0) ? 0 : Median(group.Allocations))
                    .Take(10);
                foreach (var method in methodSamples)
                    output.WriteLine($"  Hot method body: {method.Name["LLVM method: ".Length..]}: {Median(method.Times):F1} ms; " +
                                     (method.Allocations.Any(value => value < 0) ? "n/a allocations" : $"{Median(method.Allocations):N0} B"));
            }
        }
        finally
        {
            try { Directory.Delete(temporaryRoot, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static async Task<(CompilerBuildResult Result, double ElapsedMilliseconds, IReadOnlyDictionary<string, CompilationPhaseSample> Phases, long IrBytes)> RunSample(CompilerBuildRequest request)
    {
        var profiler = new CompilationProfiler();
        var stopwatch = Stopwatch.StartNew();
        var result = await LumaBuild.BuildProfiledAsync(request, profiler);
        stopwatch.Stop();
        var phases = profiler.Snapshot()
            .GroupBy(sample => sample.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => new CompilationPhaseSample(
                group.Key,
                TimeSpan.FromTicks(group.Sum(sample => sample.Elapsed.Ticks)),
                group.All(sample => sample.AllocatedBytes >= 0) ? group.Sum(sample => sample.AllocatedBytes) : -1,
                group.Sum(sample => sample.Gen0Collections),
                group.Sum(sample => sample.Gen1Collections),
                group.Sum(sample => sample.Gen2Collections)), StringComparer.Ordinal);
        var irPath = Path.Combine(request.IntermediateDirectory, request.OutputName + ".ll");
        var irBytes = File.Exists(irPath) ? new FileInfo(irPath).Length : 0;
        return (result, stopwatch.Elapsed.TotalMilliseconds, phases, irBytes);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(Environment.CurrentDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "LumaSharp.slnx")) && Directory.Exists(Path.Combine(directory.FullName, "LumaSharp.Library")))
                return directory.FullName;
        throw new DirectoryNotFoundException("Run the benchmark from inside the LumaSharp repository.");
    }

    private static double Median(List<double> values) => Median(values.ToArray());

    private static double Median(double[] values)
    {
        var ordered = values.Order().ToArray();
        var middle = ordered.Length / 2;
        return ordered.Length % 2 == 0 ? (ordered[middle - 1] + ordered[middle]) / 2 : ordered[middle];
    }

    private static double Median(long[] values) => Median(values.Select(value => (double)value).ToArray());

    private static double Median(int[] values) => Median(values.Select(value => (double)value).ToArray());
}
