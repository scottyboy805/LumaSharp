namespace LumaSharp.Benchmarks;

internal static class CSharpListToArrayBenchmark
{
    public static ListToArrayBenchmarkResult Run(int itemCount, int copies)
    {
        var list = new List<int>(4);
        for (var index = 0; index < itemCount; index++)
            list.Add(ValueAt(index));

        if (list.Capacity <= itemCount)
            throw new InvalidOperationException("The benchmark must exercise a list whose capacity exceeds its count.");

        var validation = list.ToArray();
        for (var index = 0; index < itemCount; index++)
            if (validation[index] != ValueAt(index))
                throw new InvalidOperationException($"C# ToArray returned an incorrect value at index {index}.");

        var checksum = 0;
        var timer = System.Diagnostics.Stopwatch.StartNew();
        for (var iteration = 0; iteration < copies; iteration++)
        {
            var array = list.ToArray();
            checksum = (checksum + array[0] + array[itemCount / 2] + array[itemCount - 1]) % 65_521;
            GC.KeepAlive(array);
        }

        timer.Stop();
        GC.KeepAlive(list);
        return new(checksum, timer.Elapsed.TotalMilliseconds);
    }

    private static int ValueAt(int index) => (index * 31 + 17) % 65_521;
}

internal readonly record struct ListToArrayBenchmarkResult(int Checksum, double ElapsedMilliseconds);
