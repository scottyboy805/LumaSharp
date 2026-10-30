namespace LumaSharp.Benchmarks;

internal static class CSharpListBenchmark
{
    public static ListBenchmarkResult Run(int iterations)
    {
        var list = new List<int>(4);
        var checksum = 0;
        var timer = System.Diagnostics.Stopwatch.StartNew();
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

        }

        timer.Stop();
        list.Clear();
        GC.KeepAlive(list);
        return new(checksum, timer.Elapsed.TotalMilliseconds);
    }
}

internal readonly record struct ListBenchmarkResult(int Checksum, double ElapsedMilliseconds);
