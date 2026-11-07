using System.Diagnostics;
using System.Text;
using LumaSharp.Compiler;
using Xunit;
using Xunit.Abstractions;

namespace LumaSharp.Benchmarks;

public sealed class ArrayIndexOfBenchmarkTests(ITestOutputHelper output)
{
    private const int Width = 2_048;
    private const int Iterations = 100_000;
    private const int Samples = 9;

    private static readonly IndexOfCase[] Cases =
    [
        new("bool", "bool", "true", "false", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, true, false, m, s, c, o, f), true),
        new("char", "char", "'a'", "'b'", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, 'a', 'b', m, s, c, o, f)),
        new("int8", "int8", "(int8)1", "(int8)2", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, (sbyte)1, (sbyte)2, m, s, c, o, f)),
        new("uint8", "uint8", "(uint8)1", "(uint8)2", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, (byte)1, (byte)2, m, s, c, o, f)),
        new("int16", "int16", "(int16)1", "(int16)2", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, (short)1, (short)2, m, s, c, o, f)),
        new("uint16", "uint16", "(uint16)1", "(uint16)2", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, (ushort)1, (ushort)2, m, s, c, o, f)),
        new("int32", "int32", "1", "2", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, 1, 2, m, s, c, o, f)),
        new("uint32", "uint32", "(uint32)1", "(uint32)2", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, 1u, 2u, m, s, c, o, f)),
        new("int64", "int64", "(int64)1", "(int64)2", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, 1L, 2L, m, s, c, o, f)),
        new("uint64", "uint64", "(uint64)1", "(uint64)2", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, 1UL, 2UL, m, s, c, o, f)),
        new("int", "int", "(int)1", "(int)2", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, (nint)1, (nint)2, m, s, c, o, f)),
        new("uint", "uint", "(uint)1", "(uint)2", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, (nuint)1, (nuint)2, m, s, c, o, f)),
        new("float", "float", "1.0f", "2.0f", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, 1.0f, 2.0f, m, s, c, o, f)),
        new("double", "double", "1.0", "2.0", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, 1.0, 2.0, m, s, c, o, f)),
        new("enum", "ArrayIndexOfBenchmarkShade", "ArrayIndexOfBenchmarkShade.One", "ArrayIndexOfBenchmarkShade.Two", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, ArrayIndexOfBenchmarkShade.One, ArrayIndexOfBenchmarkShade.Two, m, s, c, o, f)),
        new("nullable<int32>", "int32?", "GetNullableOne()", "GetNullableTwo()", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run<int?>(w, n, 1, 2, m, s, c, o, f)),
        new("string", "string?", "\"needle\"", "\"absent\"", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, "needle", "absent", m, s, c, o, f)),
        new("user struct", "ArrayIndexOfBenchmarkPoint", "ArrayIndexOfBenchmarkPoint(1)", "ArrayIndexOfBenchmarkPoint(2)", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, new ArrayIndexOfBenchmarkPoint(1), new ArrayIndexOfBenchmarkPoint(2), m, s, c, o, f)),
        new("user class equality", "ArrayIndexOfBenchmarkReference?", "new ArrayIndexOfBenchmarkReference(1)", "new ArrayIndexOfBenchmarkReference(2)", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, new ArrayIndexOfBenchmarkReference(1), new ArrayIndexOfBenchmarkReference(2), m, s, c, o, f)),
        new("user class equality (scope)", "ArrayIndexOfBenchmarkScopedReference?", "new ArrayIndexOfBenchmarkScopedReference(1)", "new ArrayIndexOfBenchmarkScopedReference(2)", static (w, n, m, s, c, o, f) => CSharpArrayIndexOfBenchmark.Run(w, n, new ArrayIndexOfBenchmarkReference(1), new ArrayIndexOfBenchmarkReference(2), m, s, c, o, f))
    ];

    private static readonly SearchStyle[] SearchStyles =
    [
        new("full", 0, "Array.IndexOf(values, needle)"),
        new("startIndex", 1, "Array.IndexOf(values, needle, width / 2)"),
        new("section", 2, "Array.IndexOf(values, needle, width / 2, width - width / 2)")
    ];

    [Fact]
    public async Task CompareOptimizedArrayIndexOfWithReleaseCSharpAcrossElementTypes()
    {
        var repositoryRoot = FindRepositoryRoot();
        var runtimeSources = Directory.EnumerateFiles(Path.Combine(repositoryRoot, "LumaSharp.Library"), "*.ls", SearchOption.AllDirectories).ToArray();
        var temporaryRoot = Path.Combine(Path.GetTempPath(), "lumasharp-array-indexof-benchmark-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryRoot);

        try
        {
            var generatedSource = Path.Combine(temporaryRoot, "ArrayIndexOfBenchmark.ls");
            var generatedMain = Path.Combine(temporaryRoot, "BenchmarkMain.ls");
            await File.WriteAllTextAsync(generatedSource, GenerateLumaBenchmarkSource());
            await File.WriteAllTextAsync(generatedMain, GenerateLumaBenchmarkMain());
            var build = await LumaBuild.BuildAsync(new CompilerBuildRequest(
                [generatedSource, generatedMain, .. runtimeSources],
                new CompilationOptions(UnsafeEnabled: true, Optimize: true, TargetCpu: "native"),
                Path.Combine(temporaryRoot, "bin"),
                Path.Combine(temporaryRoot, "obj"),
                "ArrayIndexOfBenchmark"));
            Assert.True(build.Succeeded, string.Join(Environment.NewLine, build.Diagnostics));
            var executable = Assert.Single(build.OutputPaths);

            var warmupCSharp = RunCSharpAll();
            for (var warmup = 0; warmup < 8; warmup++)
                _ = RunCSharpAll();
            var warmupLuma = await RunNativeAsync(executable);
            Assert.Equal(0, warmupLuma.ExitCode);
            Assert.True(string.IsNullOrWhiteSpace(warmupLuma.Error), warmupLuma.Error);
            AssertEqualChecksums(warmupCSharp, warmupLuma.Measurements);

            var timings = Cases
                .SelectMany(item => SearchStyles.SelectMany(style => new[] { (item.Name, style.Name, Missing: false), (item.Name, style.Name, Missing: true) }))
                .ToDictionary(key => key, _ => (CSharp: new List<double>(), Luma: new List<double>()));
            for (var sample = 0; sample < Samples; sample++)
            {
                ArrayIndexOfMeasurement[] csharp;
                (int ExitCode, ArrayIndexOfMeasurement[] Measurements, string Error) luma;
                if ((sample & 1) == 0)
                {
                    csharp = RunCSharpAll();
                    luma = await RunNativeAsync(executable);
                }
                else
                {
                    luma = await RunNativeAsync(executable);
                    csharp = RunCSharpAll();
                }
                Assert.Equal(0, luma.ExitCode);
                Assert.True(string.IsNullOrWhiteSpace(luma.Error), luma.Error);
                AssertEqualChecksums(csharp, luma.Measurements);
                var measurementIndex = 0;
                foreach (var item in Cases)
                {
                    foreach (var style in SearchStyles)
                    {
                        foreach (var missing in new[] { false, true })
                        {
                            var csharpMeasurement = csharp[measurementIndex++];
                            var lumaMeasurement = luma.Measurements[measurementIndex - 1];
                            var timing = timings[(item.Name, style.Name, missing)];
                            timing.CSharp.Add(csharpMeasurement.ElapsedMilliseconds);
                            timing.Luma.Add(lumaMeasurement.ElapsedMilliseconds);
                        }
                    }
                }
            }

            output.WriteLine($"Array.IndexOf overload comparison: {Width:N0} elements, {Iterations:N0} searches per case, {Samples} measured samples after warmup.");
            output.WriteLine("C# runs under the Release JIT. Luma is compiled with Optimize=true (LLVM O2). Array allocation and initialization are outside the timed region.");
            output.WriteLine("Type | Search | C# median | Luma median | C# / Luma");
            foreach (var item in Cases)
            {
                foreach (var style in SearchStyles)
                {
                    WriteResult(item.Name, style.Name + " hit", timings[(item.Name, style.Name, false)]);
                    WriteResult(item.Name, style.Name + " miss", timings[(item.Name, style.Name, true)]);
                }
            }

            void WriteResult(string name, string search, (List<double> CSharp, List<double> Luma) samples)
            {
                var csharpMedian = Median(samples.CSharp);
                var lumaMedian = Median(samples.Luma);
                output.WriteLine($"{name} | {search} | {csharpMedian:F3} ms | {lumaMedian:F3} ms | {csharpMedian / lumaMedian:F2}x");
            }
        }
        finally
        {
            try { Directory.Delete(temporaryRoot, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static ArrayIndexOfMeasurement[] RunCSharpAll() => Cases
        .SelectMany(item => SearchStyles.SelectMany(style => new[]
        {
            item.Run(Width, Iterations, false, Width / 2, Width - Width / 2, style.Overload, item.FillWithPresentWhenMissing),
            item.Run(Width, Iterations, true, Width / 2, Width - Width / 2, style.Overload, item.FillWithPresentWhenMissing)
        }))
        .ToArray();

    private static void AssertEqualChecksums(ArrayIndexOfMeasurement[] csharp, ArrayIndexOfMeasurement[] luma)
    {
        Assert.Equal(csharp.Length, luma.Length);
        for (var index = 0; index < csharp.Length; index++)
            Assert.Equal(csharp[index].Checksum, luma[index].Checksum);
    }

    private static string GenerateLumaBenchmarkSource()
    {
        var source = new StringBuilder("using System; using System.Diagnostics; enum ArrayIndexOfBenchmarkShade : uint8 { Zero, One, Two } public struct ArrayIndexOfBenchmarkPoint : System.IEquality<ArrayIndexOfBenchmarkPoint> { public int32 Value; public this(int32 value) { Value = value; } public bool Equals(ArrayIndexOfBenchmarkPoint other) => Value == other.Value; } public class ArrayIndexOfBenchmarkReference : System.IEquality<ArrayIndexOfBenchmarkReference?> { public int32 Value; public this(int32 value) { Value = value; } public bool Equals(ArrayIndexOfBenchmarkReference? other) => other != null && Value == other.Value; } public class ArrayIndexOfBenchmarkScopedReference : System.IEquality<ArrayIndexOfBenchmarkScopedReference?> { public int32 Value; public this(int32 value) { Value = value; } public bool Equals(scope ArrayIndexOfBenchmarkScopedReference? other) => other != null && Value == other.Value; } public class ArrayIndexOfBenchmark { public static int64 ElapsedTicks; private static int32? GetNullableOne() => 1; private static int32? GetNullableTwo() => 2; ");
        for (var index = 0; index < Cases.Length; index++)
        {
            var item = Cases[index];
            foreach (var style in SearchStyles)
            {
                source.Append($"public static int32 Run{index}{style.Name}(int32 width, int32 iterations, bool missing) {{ {item.LumaType}[] values = new {item.LumaType}[width]; ");
                if (item.FillWithPresentWhenMissing)
                    source.Append($"if (missing) for (int32 fillIndex = 0; fillIndex < width; fillIndex++) values[fillIndex] = {item.Present}; ");
                source.Append($"values[width - 1] = {item.Present}; {item.LumaType} needle = missing ? {item.Missing} : {item.Present}; int32 checksum = 0; StopWatch timer = StopWatch.StartNew(); for (int32 i = 0; i < iterations; i++) checksum += {style.Invocation}; timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum; }}");
            }
        }
        source.Append('}');
        return source.ToString();
    }

    private static string GenerateLumaBenchmarkMain()
    {
        var source = new StringBuilder("using System; public class Program { public static int32 Main() { ");
        for (var index = 0; index < Cases.Length; index++)
        {
            foreach (var style in SearchStyles)
            {
                foreach (var missing in new[] { false, true })
                {
                    var label = $"case{index}{style.Name}{(missing ? "Miss" : "Hit")}";
                    source.Append($"int32 {label} = ArrayIndexOfBenchmark.Run{index}{style.Name}({Width}, {Iterations}, {(missing ? "true" : "false")}); Console.WriteLine({label}.ToString()); Console.WriteLine(ArrayIndexOfBenchmark.ElapsedTicks.ToString()); ");
                }
            }
        }
        source.Append("return 0; } }");
        return source.ToString();
    }

    private static async Task<(int ExitCode, ArrayIndexOfMeasurement[] Measurements, string Error)> RunNativeAsync(string executable)
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
        if (values.Length % 2 != 0)
            return (process.ExitCode, [], $"Unexpected benchmark output: '{string.Join(" | ", values)}'. {await standardError}");
        var measurements = new ArrayIndexOfMeasurement[values.Length / 2];
        for (var index = 0; index < measurements.Length; index++)
        {
            if (!int.TryParse(values[index * 2], out var checksum) || !long.TryParse(values[index * 2 + 1], out var ticks))
                return (process.ExitCode, [], $"Unexpected benchmark output: '{string.Join(" | ", values)}'. {await standardError}");
            measurements[index] = new(checksum, ticks / 10_000.0);
        }
        return (process.ExitCode, measurements, await standardError);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(Environment.CurrentDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "LumaSharp.slnx")) && Directory.Exists(Path.Combine(directory.FullName, "LumaSharp.Library")))
                return directory.FullName;
        throw new DirectoryNotFoundException("Run the benchmark from inside the LumaSharp repository.");
    }

    private static double Median(List<double> values)
    {
        var ordered = values.Order().ToArray();
        var middle = ordered.Length / 2;
        return ordered.Length % 2 == 0 ? (ordered[middle - 1] + ordered[middle]) / 2 : ordered[middle];
    }

    private sealed record IndexOfCase(string Name, string LumaType, string Present, string Missing,
        Func<int, int, bool, int, int, int, bool, ArrayIndexOfMeasurement> Run, bool FillWithPresentWhenMissing = false);
    private sealed record SearchStyle(string Name, int Overload, string Invocation);
}
