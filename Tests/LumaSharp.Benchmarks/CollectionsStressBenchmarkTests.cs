using System.Diagnostics;
using LumaSharp.Compiler;
using Xunit;
using Xunit.Abstractions;

namespace LumaSharp.Benchmarks;

public sealed class CollectionsStressBenchmarkTests(ITestOutputHelper output)
{
    private const int Samples = 5;

    [Fact]
    public async Task CompareCollectionApiStressWorkloadsWithCSharp()
    {
        var repositoryRoot = FindRepositoryRoot();
        var fixturePath = Path.Combine(repositoryRoot, "Tests", "LumaSharp.Benchmarks", "Fixtures", "CollectionsStressBenchmark.ls");
        var runtimeSources = Directory.EnumerateFiles(Path.Combine(repositoryRoot, "LumaSharp.Library"), "*.ls", SearchOption.AllDirectories).ToArray();
        var temporaryRoot = Path.Combine(Path.GetTempPath(), "lumasharp-collections-stress-" + Guid.NewGuid().ToString("N"));
        var generatedMain = Path.Combine(temporaryRoot, "BenchmarkMain.ls");
        Directory.CreateDirectory(temporaryRoot);

        try
        {
            await File.WriteAllTextAsync(generatedMain, CreateMainSource());
            var build = await LumaBuild.BuildAsync(new CompilerBuildRequest(
                [fixturePath, generatedMain, .. runtimeSources],
                new CompilationOptions(UnsafeEnabled: true, Optimize: true),
                Path.Combine(temporaryRoot, "bin"),
                Path.Combine(temporaryRoot, "obj"),
                "CollectionsStressBenchmark"));
            Assert.True(build.Succeeded, string.Join(Environment.NewLine, build.Diagnostics));
            var executable = Assert.Single(build.OutputPaths);

            var csharpWarmup = CSharpCollectionsStressBenchmark.RunAll();
            var lumaWarmup = await RunNativeAsync(executable);
            Assert.True(lumaWarmup.ExitCode == 0, lumaWarmup.Error);
            AssertChecksums(csharpWarmup, lumaWarmup.Result);

            var csharpSamples = new List<CollectionsStressResult>(Samples);
            var lumaSamples = new List<NativeCollectionsStressResult>(Samples);
            for (var sample = 0; sample < Samples; sample++)
            {
                var csharp = CSharpCollectionsStressBenchmark.RunAll();
                var luma = await RunNativeAsync(executable);
                Assert.True(luma.ExitCode == 0, luma.Error);
                AssertChecksums(csharp, luma.Result);
                csharpSamples.Add(csharp);
                lumaSamples.Add(luma.Result);
            }

            Report("List", csharpSamples.Select(sample => sample.List.ElapsedMilliseconds).ToArray(), lumaSamples.Select(sample => sample.List.ElapsedMilliseconds).ToArray());
            Report("Stack", csharpSamples.Select(sample => sample.Stack.ElapsedMilliseconds).ToArray(), lumaSamples.Select(sample => sample.Stack.ElapsedMilliseconds).ToArray());
            Report("Queue", csharpSamples.Select(sample => sample.Queue.ElapsedMilliseconds).ToArray(), lumaSamples.Select(sample => sample.Queue.ElapsedMilliseconds).ToArray());
            Report("HashSet", csharpSamples.Select(sample => sample.HashSet.ElapsedMilliseconds).ToArray(), lumaSamples.Select(sample => sample.HashSet.ElapsedMilliseconds).ToArray());
            Report("Dictionary", csharpSamples.Select(sample => sample.Dictionary.ElapsedMilliseconds).ToArray(), lumaSamples.Select(sample => sample.Dictionary.ElapsedMilliseconds).ToArray());
            output.WriteLine($"Workloads: List/Stack/Queue each {CSharpCollectionsStressBenchmark.ListCycles:N0} cycles × {CSharpCollectionsStressBenchmark.SequenceWidth}; HashSet/Dictionary each {CSharpCollectionsStressBenchmark.SetCycles:N0} cycles × {CSharpCollectionsStressBenchmark.SetWidth}; {Samples} measured samples plus one warmup.");
            output.WriteLine("Each timer includes only collection construction and the stated API operations. Compilation, executable startup, and the C# process startup are excluded. Checksums are compared for every warmup and sample.");
            output.WriteLine("The benchmark uses the common API surface; Luma-only Ptr, Capacity on hash collections, and indexed GetAt operations have no identical BCL counterpart and are omitted.");
        }
        finally
        {
            try { Directory.Delete(temporaryRoot, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static string CreateMainSource() => $$"""
        using System;
        public class Program
        {
            public static int32 Main()
            {
                int32 listChecksum = CollectionsStressBenchmark.RunList({{CSharpCollectionsStressBenchmark.ListCycles}}, {{CSharpCollectionsStressBenchmark.SequenceWidth}});
                Console.WriteLine(listChecksum.ToString()); Console.WriteLine(CollectionsStressBenchmark.ListTicks.ToString());
                int32 stackChecksum = CollectionsStressBenchmark.RunStack({{CSharpCollectionsStressBenchmark.ListCycles}}, {{CSharpCollectionsStressBenchmark.SequenceWidth}});
                Console.WriteLine(stackChecksum.ToString()); Console.WriteLine(CollectionsStressBenchmark.StackTicks.ToString());
                int32 queueChecksum = CollectionsStressBenchmark.RunQueue({{CSharpCollectionsStressBenchmark.ListCycles}}, {{CSharpCollectionsStressBenchmark.SequenceWidth}});
                Console.WriteLine(queueChecksum.ToString()); Console.WriteLine(CollectionsStressBenchmark.QueueTicks.ToString());
                int32 hashSetChecksum = CollectionsStressBenchmark.RunHashSet({{CSharpCollectionsStressBenchmark.SetCycles}}, {{CSharpCollectionsStressBenchmark.SetWidth}});
                Console.WriteLine(hashSetChecksum.ToString()); Console.WriteLine(CollectionsStressBenchmark.HashSetTicks.ToString());
                int32 dictionaryChecksum = CollectionsStressBenchmark.RunDictionary({{CSharpCollectionsStressBenchmark.SetCycles}}, {{CSharpCollectionsStressBenchmark.SetWidth}});
                Console.WriteLine(dictionaryChecksum.ToString()); Console.WriteLine(CollectionsStressBenchmark.DictionaryTicks.ToString());
                return 0;
            }
        }
        """;

    private static void AssertChecksums(CollectionsStressResult csharp, NativeCollectionsStressResult luma)
    {
        Assert.Equal(csharp.List.Checksum, luma.List.Checksum);
        Assert.Equal(csharp.Stack.Checksum, luma.Stack.Checksum);
        Assert.Equal(csharp.Queue.Checksum, luma.Queue.Checksum);
        Assert.Equal(csharp.HashSet.Checksum, luma.HashSet.Checksum);
        Assert.Equal(csharp.Dictionary.Checksum, luma.Dictionary.Checksum);
    }

    private void Report(string name, double[] csharpSamples, double[] lumaSamples)
    {
        var csharpMedian = Median(csharpSamples);
        var lumaMedian = Median(lumaSamples);
        var ratio = csharpMedian / lumaMedian;
        var finding = ratio > 1.05 ? "LumaSharp faster" : ratio < 0.95 ? "C# faster" : "close";
        output.WriteLine($"{name}: C# median {csharpMedian:F3} ms ({csharpSamples.Min():F3}–{csharpSamples.Max():F3}); LumaSharp median {lumaMedian:F3} ms ({lumaSamples.Min():F3}–{lumaSamples.Max():F3}); C#/Luma {ratio:F2}x — {finding}.");
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(Environment.CurrentDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "LumaSharp.slnx")) && Directory.Exists(Path.Combine(directory.FullName, "LumaSharp.Library")))
                return directory.FullName;
        throw new DirectoryNotFoundException("Run the benchmark from inside the LumaSharp repository.");
    }

    private static async Task<(int ExitCode, NativeCollectionsStressResult Result, string Error)> RunNativeAsync(string executable)
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
        if (values.Length != 10)
            return (process.ExitCode, default, $"Expected ten output lines, received {values.Length}; native exit code {process.ExitCode}; partial output '{string.Join(" | ", values)}'. {await standardError}");

        var measurements = new CollectionMeasurement[5];
        for (var index = 0; index < measurements.Length; index++)
        {
            if (!int.TryParse(values[index * 2], out var checksum) || !long.TryParse(values[index * 2 + 1], out var ticks))
                return (process.ExitCode, default, $"Invalid output pair {index + 1}: '{values[index * 2]}', '{values[index * 2 + 1]}'. {await standardError}");
            measurements[index] = new(checksum, ticks / 10_000.0);
        }

        return (process.ExitCode, new(measurements[0], measurements[1], measurements[2], measurements[3], measurements[4]), await standardError);
    }

    private static double Median(double[] values)
    {
        var ordered = values.Order().ToArray();
        return ordered[ordered.Length / 2];
    }

    private readonly record struct CollectionMeasurement(int Checksum, double ElapsedMilliseconds);
    private readonly record struct NativeCollectionsStressResult(CollectionMeasurement List, CollectionMeasurement Stack, CollectionMeasurement Queue, CollectionMeasurement HashSet, CollectionMeasurement Dictionary);
}
