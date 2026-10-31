using System.Collections.Generic;

namespace LumaSharp.Benchmarks;

internal static class CSharpCollectionsStressBenchmark
{
    public const int ListCycles = 10_000;
    public const int SequenceWidth = 64;
    public const int SetCycles = 1_500;
    public const int SetWidth = 128;
    private const int ChecksumModulus = 1_000_003;

    public static CollectionsStressResult RunAll() => new(
        RunList(ListCycles, SequenceWidth),
        RunStack(ListCycles, SequenceWidth),
        RunQueue(ListCycles, SequenceWidth),
        RunHashSet(SetCycles, SetWidth),
        RunDictionary(SetCycles, SetWidth));

    private static CollectionMeasurement RunList(int cycles, int width)
    {
        var list = new List<int>();
        var checksum = 0;
        var timer = System.Diagnostics.Stopwatch.StartNew();
        for (var cycle = 0; cycle < cycles; cycle++)
        {
            list.Clear();
            var start = cycle * width;
            for (var index = 0; index < width; index++)
                list.Add(start + index);

            var sum = list.Count + list.Capacity + list[0] + list[width - 1];
            list[width / 2] = start - 1;
            sum += list.IndexOf(start - 1);
            if (list.Contains(start + width - 1)) sum++;
            list.Insert(width / 4, start - 2);
            sum += list[width / 4];
            list.RemoveAt(width / 4);
            if (list.Remove(start + width - 1)) sum++;
            var copy = list.ToArray();
            sum += copy[0] + copy[copy.Length / 2] + copy[copy.Length - 1];
            foreach (var item in list)
                sum += item;
            checksum = (checksum + sum) % ChecksumModulus;
        }
        timer.Stop();
        GC.KeepAlive(list);
        return new(checksum, timer.Elapsed.TotalMilliseconds);
    }

    private static CollectionMeasurement RunStack(int cycles, int width)
    {
        var stack = new Stack<int>();
        var checksum = 0;
        var timer = System.Diagnostics.Stopwatch.StartNew();
        for (var cycle = 0; cycle < cycles; cycle++)
        {
            stack.Clear();
            var start = cycle * width;
            for (var index = 0; index < width; index++)
                stack.Push(start + index);

            var sum = stack.Count + stack.Peek();
            foreach (var item in stack)
                sum += item;
            while (stack.Count > 0)
                sum += stack.Pop();
            checksum = (checksum + sum) % ChecksumModulus;
        }
        timer.Stop();
        GC.KeepAlive(stack);
        return new(checksum, timer.Elapsed.TotalMilliseconds);
    }

    private static CollectionMeasurement RunQueue(int cycles, int width)
    {
        var queue = new Queue<int>();
        var checksum = 0;
        var timer = System.Diagnostics.Stopwatch.StartNew();
        for (var cycle = 0; cycle < cycles; cycle++)
        {
            queue.Clear();
            var start = cycle * width;
            for (var index = 0; index < width; index++)
                queue.Enqueue(start + index);

            var sum = queue.Count + queue.Peek();
            for (var index = 0; index < width / 2; index++)
                sum += queue.Dequeue();
            for (var index = 0; index < width / 2; index++)
                queue.Enqueue(start + width + index);
            foreach (var item in queue)
                sum += item;
            while (queue.Count > 0)
                sum += queue.Dequeue();
            checksum = (checksum + sum) % ChecksumModulus;
        }
        timer.Stop();
        GC.KeepAlive(queue);
        return new(checksum, timer.Elapsed.TotalMilliseconds);
    }

    private static CollectionMeasurement RunHashSet(int cycles, int width)
    {
        var set = new HashSet<int>();
        var checksum = 0;
        var timer = System.Diagnostics.Stopwatch.StartNew();
        for (var cycle = 0; cycle < cycles; cycle++)
        {
            set.Clear();
            var start = cycle * width;
            var sum = 0;
            for (var index = 0; index < width; index++)
            {
                if (set.Add(start + index)) sum++;
                if (!set.Add(start + index)) sum++;
            }
            sum += set.Count;
            for (var index = 0; index < width; index += 2)
            {
                if (set.Contains(start + index)) sum++;
                if (set.Remove(start + index)) sum++;
                if (!set.Remove(start + index)) sum++;
                if (set.Add(start + width + index)) sum++;
            }
            foreach (var item in set)
                sum += item;
            checksum = (checksum + sum) % ChecksumModulus;
        }
        timer.Stop();
        GC.KeepAlive(set);
        return new(checksum, timer.Elapsed.TotalMilliseconds);
    }

    private static CollectionMeasurement RunDictionary(int cycles, int width)
    {
        var dictionary = new Dictionary<int, int>();
        var checksum = 0;
        var timer = System.Diagnostics.Stopwatch.StartNew();
        for (var cycle = 0; cycle < cycles; cycle++)
        {
            dictionary.Clear();
            var start = cycle * width;
            var sum = 0;
            for (var index = 0; index < width; index++)
            {
                var key = start + index;
                dictionary.Add(key, key * 3);
                if (!dictionary.TryAdd(key, 0)) sum++;
            }
            sum += dictionary.Count;
            for (var index = 0; index < width; index++)
            {
                var key = start + index;
                if (dictionary.ContainsKey(key)) sum++;
                if (dictionary.TryGetValue(key, out var value)) sum += value;
                if ((index & 1) == 0)
                {
                    dictionary[key] = value + 1;
                    sum += dictionary[key];
                }
                else
                {
                    if (dictionary.Remove(key)) sum++;
                    if (!dictionary.Remove(key)) sum++;
                    dictionary[key] = value + 2;
                }
            }
            if (dictionary.TryGetValue(start + width, out _)) sum++;
            checksum = (checksum + sum) % ChecksumModulus;
        }
        timer.Stop();
        GC.KeepAlive(dictionary);
        return new(checksum, timer.Elapsed.TotalMilliseconds);
    }
}

internal readonly record struct CollectionMeasurement(int Checksum, double ElapsedMilliseconds);

internal sealed record CollectionsStressResult(
    CollectionMeasurement List,
    CollectionMeasurement Stack,
    CollectionMeasurement Queue,
    CollectionMeasurement HashSet,
    CollectionMeasurement Dictionary);
