using System.Collections.Generic;

namespace LumaSharp.Benchmarks;

internal static class CSharpCollectionApiBenchmark
{
    private const int Modulus = 1_000_003;

    public static CollectionApiMeasurement RunCase(int operation, int iterations, int width)
    {
        var checksum = 0;
        switch (operation)
        {
            case 0:
            {
                var list = new List<int>(iterations);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var index = 0; index < iterations; index++) list.Add(index);
                timer.Stop();
                checksum = list.Count;
                return Result(checksum, timer);
            }
            case 1:
            case 2:
            case 3:
            case 4:
            {
                var list = new List<int>(width);
                for (var index = 0; index < width; index++) list.Add(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var index = 0; index < iterations; index++)
                {
                    var slot = index & (width - 1);
                    checksum += operation switch
                    {
                        1 => list.Count,
                        2 => list.Capacity,
                        3 => list[slot],
                        _ => SetAndRead(list, slot, index)
                    };
                    checksum %= Modulus;
                }
                timer.Stop();
                return Result(checksum, timer);
            }
            case 5:
            case 6:
            case 7:
            case 8:
            {
                var list = new List<int>(width);
                for (var index = 0; index < width; index++) list.Add(index);
                var item = operation is 5 or 7 ? width - 1 : -1;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var index = 0; index < iterations; index++)
                {
                    var found = operation is 5 or 6 ? list.IndexOf(item) : list.Contains(item) ? 1 : 0;
                    checksum = (checksum + found) % Modulus;
                }
                timer.Stop();
                return Result(checksum, timer);
            }
            case 9:
            case 10:
            {
                var list = new List<int>(width + 1);
                for (var index = 0; index < width; index++) list.Add(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var index = 0; index < iterations; index++)
                {
                    if (operation == 9)
                    {
                        list.Insert(width / 2, -1);
                        list.RemoveAt(width / 2);
                    }
                    else
                    {
                        if (list.Remove(width - 1)) checksum++;
                        list.Add(width - 1);
                    }
                }
                timer.Stop();
                checksum = (checksum + list.Count) % Modulus;
                return Result(checksum, timer);
            }
            case 11:
            {
                var list = new List<int>(width);
                for (var index = 0; index < width; index++) list.Add(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                list.Clear();
                timer.Stop();
                return Result(list.Count, timer);
            }
            case 12:
            {
                var list = new List<int>(width);
                for (var index = 0; index < width; index++) list.Add(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var iteration = 0; iteration < iterations; iteration++)
                {
                    var sum = 0;
                    foreach (var item in list) sum += item;
                    checksum = (checksum + sum) % Modulus;
                }
                timer.Stop();
                return Result(checksum, timer);
            }
            case 47:
            case 48:
            case 49:
            {
                var lists = new List<int>[iterations];
                for (var index = 0; index < iterations; index++)
                {
                    var list = new List<int>(width + 1);
                    for (var item = 0; item < width; item++) list.Add(item);
                    lists[index] = list;
                }
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var index = 0; index < iterations; index++)
                {
                    var list = lists[index];
                    if (operation == 47) list.Insert(width / 2, -1);
                    else if (operation == 48) list.RemoveAt(width / 2);
                    else if (list.Remove(width - 1)) checksum++;
                    checksum = (checksum + list.Count) % Modulus;
                }
                timer.Stop();
                return Result(checksum, timer);
            }
            case 13:
            {
                var stack = new Stack<int>(iterations);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var index = 0; index < iterations; index++) stack.Push(index);
                timer.Stop();
                return Result(stack.Count, timer);
            }
            case 14:
            {
                var stack = new Stack<int>(iterations);
                for (var index = 0; index < iterations; index++) stack.Push(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                while (stack.Count > 0) checksum = (checksum + stack.Pop()) % Modulus;
                timer.Stop();
                return Result(checksum, timer);
            }
            case 15:
            case 16:
            {
                var stack = new Stack<int>(width);
                for (var index = 0; index < width; index++) stack.Push(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var index = 0; index < iterations; index++)
                    checksum = (checksum + (operation == 15 ? stack.Peek() : stack.Count)) % Modulus;
                timer.Stop();
                return Result(checksum, timer);
            }
            case 17:
            {
                var stack = new Stack<int>(width);
                for (var index = 0; index < width; index++) stack.Push(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                stack.Clear();
                timer.Stop();
                return Result(stack.Count, timer);
            }
            case 18:
            {
                var stack = new Stack<int>(width);
                for (var index = 0; index < width; index++) stack.Push(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var iteration = 0; iteration < iterations; iteration++)
                {
                    var sum = 0;
                    foreach (var item in stack) sum += item;
                    checksum = (checksum + sum) % Modulus;
                }
                timer.Stop();
                return Result(checksum, timer);
            }
            case 19:
            {
                var queue = new Queue<int>(iterations);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var index = 0; index < iterations; index++) queue.Enqueue(index);
                timer.Stop();
                return Result(queue.Count, timer);
            }
            case 20:
            {
                var queue = new Queue<int>(iterations);
                for (var index = 0; index < iterations; index++) queue.Enqueue(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                while (queue.Count > 0) checksum = (checksum + queue.Dequeue()) % Modulus;
                timer.Stop();
                return Result(checksum, timer);
            }
            case 21:
            case 22:
            {
                var queue = new Queue<int>(width);
                for (var index = 0; index < width; index++) queue.Enqueue(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var index = 0; index < iterations; index++)
                    checksum = (checksum + (operation == 21 ? queue.Peek() : queue.Count)) % Modulus;
                timer.Stop();
                return Result(checksum, timer);
            }
            case 23:
            {
                var queue = new Queue<int>(width);
                for (var index = 0; index < width; index++) queue.Enqueue(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                queue.Clear();
                timer.Stop();
                return Result(queue.Count, timer);
            }
            case 24:
            {
                var queue = new Queue<int>(width);
                for (var index = 0; index < width; index++) queue.Enqueue(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var iteration = 0; iteration < iterations; iteration++)
                {
                    var sum = 0;
                    foreach (var item in queue) sum += item;
                    checksum = (checksum + sum) % Modulus;
                }
                timer.Stop();
                return Result(checksum, timer);
            }
            case 25:
            case 26:
            case 27:
            case 28:
            case 29:
            case 30:
            case 31:
            case 32:
            {
                var set = new HashSet<int>(iterations > width ? iterations : width);
                var populateCount = operation == 29 ? iterations : width;
                if (operation == 25) populateCount = 0;
                for (var index = 0; index < populateCount; index++) set.Add(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                switch (operation)
                {
                    case 25:
                        for (var index = 0; index < iterations; index++) if (set.Add(index)) checksum++;
                        break;
                    case 26:
                        for (var index = 0; index < iterations; index++) if (!set.Add(index & (width - 1))) checksum++;
                        break;
                    case 27:
                        for (var index = 0; index < iterations; index++) if (set.Contains(index & (width - 1))) checksum++;
                        break;
                    case 28:
                        for (var index = 0; index < iterations; index++) if (set.Contains(-index - 1)) checksum++;
                        break;
                    case 29:
                        for (var index = 0; index < iterations; index++) if (set.Remove(index)) checksum++;
                        break;
                    case 30:
                        for (var index = 0; index < iterations; index++) if (set.Remove(-index - 1)) checksum++;
                        break;
                    case 31:
                        for (var index = 0; index < iterations; index++) checksum += set.Count;
                        break;
                    case 32:
                        set.Clear();
                        checksum = set.Count;
                        break;
                }
                timer.Stop();
                return Result(checksum % Modulus, timer);
            }
            case 33:
            {
                var set = new HashSet<int>(width);
                for (var index = 0; index < width; index++) set.Add(index);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var iteration = 0; iteration < iterations; iteration++)
                {
                    var sum = 0;
                    foreach (var item in set) sum += item;
                    checksum = (checksum + sum) % Modulus;
                }
                timer.Stop();
                return Result(checksum, timer);
            }
            case 34:
            case 35:
            case 36:
            case 37:
            case 38:
            case 39:
            case 40:
            case 41:
            case 42:
            case 43:
            case 44:
            case 45:
            case 46:
            {
                var dictionary = new Dictionary<int, int>(iterations > width ? iterations : width);
                var populateCount = operation == 43 ? iterations : width;
                if (operation is 34 or 36) populateCount = 0;
                for (var index = 0; index < populateCount; index++) dictionary.Add(index, index * 3);
                var timer = System.Diagnostics.Stopwatch.StartNew();
                switch (operation)
                {
                    case 34:
                        for (var index = 0; index < iterations; index++) { dictionary.Add(index, index * 3); checksum++; }
                        break;
                    case 35:
                        for (var index = 0; index < iterations; index++) if (!dictionary.TryAdd(index & (width - 1), 0)) checksum++;
                        break;
                    case 36:
                for (var index = 0; index < iterations; index++) if (dictionary.TryAdd(index, index * 3)) checksum++;
                        break;
                    case 37:
                        for (var index = 0; index < iterations; index++) if (dictionary.ContainsKey(index & (width - 1))) checksum++;
                        break;
                    case 38:
                        for (var index = 0; index < iterations; index++) if (dictionary.ContainsKey(-index - 1)) checksum++;
                        break;
                    case 39:
                        for (var index = 0; index < iterations; index++) if (dictionary.TryGetValue(index & (width - 1), out var found)) checksum += found;
                        break;
                    case 40:
                        for (var index = 0; index < iterations; index++) if (dictionary.TryGetValue(-index - 1, out var missing)) checksum += missing;
                        break;
                    case 41:
                        for (var index = 0; index < iterations; index++) checksum += dictionary[index & (width - 1)];
                        break;
                    case 42:
                        for (var index = 0; index < iterations; index++) dictionary[index & (width - 1)] = index;
                        checksum = dictionary[0];
                        break;
                    case 43:
                        for (var index = 0; index < iterations; index++) if (dictionary.Remove(index)) checksum++;
                        break;
                    case 44:
                        for (var index = 0; index < iterations; index++) if (dictionary.Remove(-index - 1)) checksum++;
                        break;
                    case 45:
                        for (var index = 0; index < iterations; index++) checksum += dictionary.Count;
                        break;
                    case 46:
                        dictionary.Clear();
                        checksum = dictionary.Count;
                        break;
                }
                timer.Stop();
                return Result(checksum % Modulus, timer);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(operation));
        }
    }

    private static int SetAndRead(List<int> list, int index, int value)
    {
        list[index] = value;
        return list[index];
    }

    private static CollectionApiMeasurement Result(int checksum, System.Diagnostics.Stopwatch timer) => new(checksum, timer.Elapsed.TotalMilliseconds);
}

internal readonly record struct CollectionApiMeasurement(int Checksum, double ElapsedMilliseconds);
