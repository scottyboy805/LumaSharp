namespace LumaSharp.Benchmarks;

internal static class CSharpListBenchmark
{
    public static int Run(int iterations)
    {
        var defaultList = new List<int>();
        var list = new List<int>(4);
        var checksum = 0;
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

            checksum = (checksum + list.ToArray().Length) % 127;
        }

        list.Clear();
        GC.KeepAlive(defaultList);
        GC.KeepAlive(list);
        return checksum;
    }
}
