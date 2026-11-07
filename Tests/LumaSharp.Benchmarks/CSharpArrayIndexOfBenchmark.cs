namespace LumaSharp.Benchmarks;

internal static class CSharpArrayIndexOfBenchmark
{
    public static ArrayIndexOfMeasurement Run<T>(int width, int iterations, T present, T missing, bool useMissing,
        int startIndex, int count, int overload, bool fillWithPresentWhenMissing = false)
    {
        var values = new T[width];
        if (useMissing && fillWithPresentWhenMissing)
        {
            for (var index = 0; index < values.Length; index++)
                values[index] = present;
        }
        values[width - 1] = present;
        var needle = useMissing ? missing : present;
        var checksum = 0;
        var timer = System.Diagnostics.Stopwatch.StartNew();
        switch (overload)
        {
            case 0:
                for (var i = 0; i < iterations; i++) checksum += Array.IndexOf(values, needle);
                break;
            case 1:
                for (var i = 0; i < iterations; i++) checksum += Array.IndexOf(values, needle, startIndex);
                break;
            case 2:
                for (var i = 0; i < iterations; i++) checksum += Array.IndexOf(values, needle, startIndex, count);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(overload));
        }
        timer.Stop();
        GC.KeepAlive(values);
        return new(checksum, timer.Elapsed.TotalMilliseconds);
    }
}

internal readonly record struct ArrayIndexOfMeasurement(int Checksum, double ElapsedMilliseconds);

internal enum ArrayIndexOfBenchmarkShade : byte { Zero, One, Two }

internal readonly struct ArrayIndexOfBenchmarkPoint(int value) : IEquatable<ArrayIndexOfBenchmarkPoint>
{
    public int Value { get; } = value;
    public bool Equals(ArrayIndexOfBenchmarkPoint other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ArrayIndexOfBenchmarkPoint other && Equals(other);
    public override int GetHashCode() => Value;
}

internal sealed class ArrayIndexOfBenchmarkReference(int value) : IEquatable<ArrayIndexOfBenchmarkReference>
{
    public int Value { get; } = value;
    public bool Equals(ArrayIndexOfBenchmarkReference? other) => other is not null && Value == other.Value;
}
