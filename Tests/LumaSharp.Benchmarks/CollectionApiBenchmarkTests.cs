using System.Diagnostics;
using LumaSharp.Compiler;
using Xunit;
using Xunit.Abstractions;

namespace LumaSharp.Benchmarks;

public sealed class CollectionApiBenchmarkTests(ITestOutputHelper output)
{
    private const int Samples = 5;
    private static readonly ApiCase[] Cases =
    [
        new("List.Add", 0, 100_000, 256),
        new("List.Count", 1, 500_000, 256),
        new("List.Capacity", 2, 500_000, 256),
        new("List[index] get", 3, 500_000, 256),
        new("List[index] set", 4, 500_000, 256),
        new("List.IndexOf hit", 5, 30_000, 256),
        new("List.IndexOf miss", 6, 30_000, 256),
        new("List.Contains hit", 7, 30_000, 256),
        new("List.Contains miss", 8, 30_000, 256),
        new("List.Insert + RemoveAt", 9, 50_000, 256),
        new("List.Remove + Add", 10, 50_000, 256),
        new("List.Clear", 11, 1, 100_000),
        new("List iteration", 12, 5_000, 128),
        new("List.Insert", 47, 1_000, 128),
        new("List.RemoveAt", 48, 1_000, 128),
        new("List.Remove hit", 49, 1_000, 128),
        new("Stack.Push", 13, 100_000, 256),
        new("Stack.Pop", 14, 100_000, 256),
        new("Stack.Peek", 15, 500_000, 256),
        new("Stack.Count", 16, 500_000, 256),
        new("Stack.Clear", 17, 1, 100_000),
        new("Stack iteration", 18, 5_000, 128),
        new("Queue.Enqueue", 19, 100_000, 256),
        new("Queue.Dequeue", 20, 100_000, 256),
        new("Queue.Peek", 21, 500_000, 256),
        new("Queue.Count", 22, 500_000, 256),
        new("Queue.Clear", 23, 1, 100_000),
        new("Queue iteration", 24, 5_000, 128),
        new("HashSet.Add new", 25, 50_000, 512),
        new("HashSet.Add duplicate", 26, 200_000, 512),
        new("HashSet.Contains hit", 27, 200_000, 512),
        new("HashSet.Contains miss", 28, 200_000, 512),
        new("HashSet.Remove hit", 29, 50_000, 512),
        new("HashSet.Remove miss", 30, 200_000, 512),
        new("HashSet.Count", 31, 500_000, 512),
        new("HashSet.Clear", 32, 1, 50_000),
        new("HashSet iteration", 33, 5_000, 128),
        new("Dictionary.Add new", 34, 50_000, 512),
        new("Dictionary.TryAdd duplicate", 35, 200_000, 512),
        new("Dictionary.TryAdd new", 36, 50_000, 512),
        new("Dictionary.ContainsKey hit", 37, 200_000, 512),
        new("Dictionary.ContainsKey miss", 38, 200_000, 512),
        new("Dictionary.TryGetValue hit", 39, 200_000, 512),
        new("Dictionary.TryGetValue miss", 40, 200_000, 512),
        new("Dictionary indexer get", 41, 200_000, 512),
        new("Dictionary indexer set", 42, 200_000, 512),
        new("Dictionary.Remove hit", 43, 50_000, 512),
        new("Dictionary.Remove miss", 44, 200_000, 512),
        new("Dictionary.Count", 45, 500_000, 512),
        new("Dictionary.Clear", 46, 1, 50_000)
    ];

    [Fact]
    public async Task CompareIndividualCollectionApisWithCSharp()
    {
        var repositoryRoot = FindRepositoryRoot();
        var fixturePath = Path.Combine(repositoryRoot, "Tests", "LumaSharp.Benchmarks", "Fixtures", "CollectionsApiBenchmark.ls");
        var runtimeSources = Directory.EnumerateFiles(Path.Combine(repositoryRoot, "LumaSharp.Library"), "*.ls", SearchOption.AllDirectories).ToArray();
        var temporaryRoot = Path.Combine(Path.GetTempPath(), "lumasharp-collection-api-benchmark-" + Guid.NewGuid().ToString("N"));
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
                "CollectionApiBenchmark"));
            Assert.True(build.Succeeded, string.Join(Environment.NewLine, build.Diagnostics));
            var executable = Assert.Single(build.OutputPaths);

            var csharpWarmup = RunCSharpAll();
            var nativeWarmup = await RunNativeAsync(executable);
            Assert.True(nativeWarmup.ExitCode == 0, nativeWarmup.Error);
            AssertChecksums(csharpWarmup, nativeWarmup.Measurements);

            var csharpSamples = Cases.ToDictionary(item => item.Id, _ => new List<double>(Samples));
            var lumaSamples = Cases.ToDictionary(item => item.Id, _ => new List<double>(Samples));
            for (var sample = 0; sample < Samples; sample++)
            {
                var csharp = RunCSharpAll();
                var native = await RunNativeAsync(executable);
                Assert.True(native.ExitCode == 0, native.Error);
                AssertChecksums(csharp, native.Measurements);
                var csharpById = Cases.Select((item, index) => (item.Id, Measurement: csharp[index]))
                    .ToDictionary(entry => entry.Id, entry => entry.Measurement);
                var nativeById = Cases.Select((item, index) => (item.Id, Measurement: native.Measurements[index]))
                    .ToDictionary(entry => entry.Id, entry => entry.Measurement);
                foreach (var item in Cases)
                {
                    csharpSamples[item.Id].Add(csharpById[item.Id].ElapsedMilliseconds);
                    lumaSamples[item.Id].Add(nativeById[item.Id].ElapsedMilliseconds);
                }
            }

            foreach (var item in Cases)
            {
                var csharp = csharpSamples[item.Id].ToArray();
                var luma = lumaSamples[item.Id].ToArray();
                var csharpMedian = Median(csharp);
                var lumaMedian = Median(luma);
                var ratio = csharpMedian / lumaMedian;
                var result = ratio > 1.05 ? "Luma faster" : ratio < 0.95 ? "C# faster" : "close";
                output.WriteLine($"{item.Name,-31} C# {csharpMedian,9:F3} ms [{csharp.Min():F3}–{csharp.Max():F3}]  Luma {lumaMedian,9:F3} ms [{luma.Min():F3}–{luma.Max():F3}]  C#/Luma {ratio,5:F2}x  {result}");
            }
            output.WriteLine("Five measured Release samples after one warmup. C# uses Release JIT; Luma is built with Optimize=true (LLVM O2). Each timer excludes collection setup; Clear cases time one Clear on a prefilled collection. Search APIs use hit-at-end and absent-key cases. Matching checksums are required for every sample.");
            output.WriteLine("List.ToArray has a separate full-array stress benchmark. Luma-only Ptr, GetAt, and hash-table Capacity have no direct BCL counterpart and are not reported here.");
        }
        finally
        {
            try { Directory.Delete(temporaryRoot, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static CollectionApiMeasurement[] RunCSharpAll() => Cases.Select(item => CSharpCollectionApiBenchmark.RunCase(item.Id, item.Iterations, item.Width)).ToArray();

    private static void AssertChecksums(CollectionApiMeasurement[] csharp, NativeCollectionApiMeasurement[] luma)
    {
        Assert.Equal(Cases.Length, luma.Length);
        for (var index = 0; index < Cases.Length; index++)
            Assert.Equal(csharp[index].Checksum, luma[index].Checksum);
    }

    private static string CreateMainSource()
    {
        var main = new System.Text.StringBuilder("using System; public class Program { public static int32 Main() {\n");
        foreach (var item in Cases)
        {
            main.Append($"int32 checksum{item.Id} = CollectionsApiBenchmark.RunCase({item.Id}, {item.Iterations}, {item.Width});\n");
            main.Append($"Console.WriteLine(checksum{item.Id}.ToString()); Console.WriteLine(CollectionsApiBenchmark.ElapsedTicks.ToString());\n");
        }
        main.Append("return 0; } }\n");
        return main.ToString();
    }

    private static async Task<(int ExitCode, NativeCollectionApiMeasurement[] Measurements, string Error)> RunNativeAsync(string executable)
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
        if (values.Length != Cases.Length * 2)
            return (process.ExitCode, [], $"Expected {Cases.Length * 2} output lines; got {values.Length}, exit {process.ExitCode}: '{string.Join(" | ", values)}'. {await standardError}");

        var measurements = new NativeCollectionApiMeasurement[Cases.Length];
        for (var index = 0; index < measurements.Length; index++)
        {
            if (!int.TryParse(values[index * 2], out var checksum) || !long.TryParse(values[index * 2 + 1], out var ticks))
                return (process.ExitCode, [], $"Invalid output pair {index}: '{values[index * 2]}' / '{values[index * 2 + 1]}'. {await standardError}");
            measurements[index] = new(checksum, ticks / 10_000.0);
        }
        return (process.ExitCode, measurements, await standardError);
    }

    private static double Median(double[] values) => values.Order().ElementAt(values.Length / 2);

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(Environment.CurrentDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "LumaSharp.slnx")) && Directory.Exists(Path.Combine(directory.FullName, "LumaSharp.Library")))
                return directory.FullName;
        throw new DirectoryNotFoundException("Run the benchmark from inside the LumaSharp repository.");
    }

    private readonly record struct ApiCase(string Name, int Id, int Iterations, int Width);
    private readonly record struct NativeCollectionApiMeasurement(int Checksum, double ElapsedMilliseconds);
}
