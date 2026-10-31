using System;
using System.Collections;
using System.Diagnostics;

public class CollectionsStressBenchmark
{
	public static int64 ListTicks;
	public static int64 StackTicks;
	public static int64 QueueTicks;
	public static int64 HashSetTicks;
	public static int64 DictionaryTicks;

	public static int32 RunList(int32 cycles, int32 width)
	{
		List<int32> list = new List<int32>();
		int32 checksum = 0;
		StopWatch timer = StopWatch.StartNew();
		for (int32 cycle = 0; cycle < cycles; cycle++)
		{
			list.Clear();
			int32 start = cycle * width;
			for (int32 index = 0; index < width; index++)
				list.Add(start + index);

			int32 sum = list.Count + list.Capacity + list[0] + list[width - 1];
			list[width / 2] = start - 1;
			sum += list.IndexOf(start - 1);
			if (list.Contains(start + width - 1)) sum++;
			list.Insert(width / 4, start - 2);
			sum += list[width / 4];
			list.RemoveAt(width / 4);
			if (list.Remove(start + width - 1)) sum++;
			int32[] copy = list.ToArray();
			sum += copy[0] + copy[copy.Length / 2] + copy[copy.Length - 1];
			for (let item in list)
				sum += item;
			checksum = (checksum + sum) % 1000003;
		}
		timer.Stop();
		ListTicks = timer.Elapsed.Ticks;
		return checksum;
	}

	public static int32 RunStack(int32 cycles, int32 width)
	{
		Stack<int32> stack = new Stack<int32>();
		int32 checksum = 0;
		StopWatch timer = StopWatch.StartNew();
		for (int32 cycle = 0; cycle < cycles; cycle++)
		{
			stack.Clear();
			int32 start = cycle * width;
			for (int32 index = 0; index < width; index++)
				stack.Push(start + index);

			int32 sum = stack.Count + stack.Peek();
			for (let item in stack)
				sum += item;
			while (stack.Count > 0)
				sum += stack.Pop();
			checksum = (checksum + sum) % 1000003;
		}
		timer.Stop();
		StackTicks = timer.Elapsed.Ticks;
		return checksum;
	}

	public static int32 RunQueue(int32 cycles, int32 width)
	{
		Queue<int32> queue = new Queue<int32>();
		int32 checksum = 0;
		StopWatch timer = StopWatch.StartNew();
		for (int32 cycle = 0; cycle < cycles; cycle++)
		{
			queue.Clear();
			int32 start = cycle * width;
			for (int32 index = 0; index < width; index++)
				queue.Enqueue(start + index);

			int32 sum = queue.Count + queue.Peek();
			for (int32 index = 0; index < width / 2; index++)
				sum += queue.Dequeue();
			for (int32 index = 0; index < width / 2; index++)
				queue.Enqueue(start + width + index);
			for (let item in queue)
				sum += item;
			while (queue.Count > 0)
				sum += queue.Dequeue();
			checksum = (checksum + sum) % 1000003;
		}
		timer.Stop();
		QueueTicks = timer.Elapsed.Ticks;
		return checksum;
	}

	public static int32 RunHashSet(int32 cycles, int32 width)
	{
		HashSet<int32> set = new HashSet<int32>();
		int32 checksum = 0;
		StopWatch timer = StopWatch.StartNew();
		for (int32 cycle = 0; cycle < cycles; cycle++)
		{
			set.Clear();
			int32 start = cycle * width;
			int32 sum = 0;
			for (int32 index = 0; index < width; index++)
			{
				if (set.Add(start + index)) sum++;
				if (!set.Add(start + index)) sum++;
			}
			sum += set.Count;
			for (int32 index = 0; index < width; index += 2)
			{
				if (set.Contains(start + index)) sum++;
				if (set.Remove(start + index)) sum++;
				if (!set.Remove(start + index)) sum++;
				if (set.Add(start + width + index)) sum++;
			}
			for (let item in set)
				sum += item;
			checksum = (checksum + sum) % 1000003;
		}
		timer.Stop();
		HashSetTicks = timer.Elapsed.Ticks;
		return checksum;
	}

	public static int32 RunDictionary(int32 cycles, int32 width)
	{
		Dictionary<int32, int32> dictionary = new Dictionary<int32, int32>();
		int32 checksum = 0;
		StopWatch timer = StopWatch.StartNew();
		for (int32 cycle = 0; cycle < cycles; cycle++)
		{
			dictionary.Clear();
			int32 start = cycle * width;
			int32 sum = 0;
			for (int32 index = 0; index < width; index++)
			{
				int32 key = start + index;
				dictionary.Add(key, key * 3);
				if (!dictionary.TryAdd(key, 0)) sum++;
			}
			sum += dictionary.Count;
			for (int32 index = 0; index < width; index++)
			{
				int32 key = start + index;
				if (dictionary.ContainsKey(key)) sum++;
				int32 value;
				if (dictionary.TryGetValue(key, out value)) sum += value;
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
			int32 missing;
			if (dictionary.TryGetValue(start + width, out missing)) sum++;
			checksum = (checksum + sum) % 1000003;
		}
		timer.Stop();
		DictionaryTicks = timer.Elapsed.Ticks;
		return checksum;
	}
}
