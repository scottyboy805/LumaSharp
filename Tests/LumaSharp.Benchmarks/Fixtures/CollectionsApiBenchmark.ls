using System;
using System.Collections;
using System.Diagnostics;

public class CollectionsApiBenchmark
{
	public static int64 ElapsedTicks;

	public static int32 RunCase(int32 operation, int32 iterations, int32 width)
	{
		int32 checksum = 0;
		if (operation == 0)
		{
			List<int32> list = new List<int32>(iterations);
			StopWatch timer = StopWatch.StartNew();
			for (int32 index = 0; index < iterations; index++) list.Add(index);
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return list.Count;
		}
		if (operation >= 1 && operation <= 4)
		{
			List<int32> list = new List<int32>(width);
			for (int32 index = 0; index < width; index++) list.Add(index);
			StopWatch timer = StopWatch.StartNew();
			for (int32 index = 0; index < iterations; index++)
			{
				int32 slot = index & (width - 1);
				if (operation == 1) checksum += list.Count;
				else if (operation == 2) checksum += list.Capacity;
				else if (operation == 3) checksum += list[slot];
				else { list[slot] = index; checksum += list[slot]; }
				checksum %= 1000003;
			}
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum;
		}
		if (operation >= 5 && operation <= 8)
		{
			List<int32> list = new List<int32>(width);
			for (int32 index = 0; index < width; index++) list.Add(index);
			int32 item = operation == 5 || operation == 7 ? width - 1 : -1;
			StopWatch timer = StopWatch.StartNew();
			for (int32 index = 0; index < iterations; index++)
			{
				if (operation == 5 || operation == 6) checksum += list.IndexOf(item);
				else if (list.Contains(item)) checksum++;
				checksum %= 1000003;
			}
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum;
		}
		if (operation == 9 || operation == 10)
		{
			List<int32> list = new List<int32>(width + 1);
			for (int32 index = 0; index < width; index++) list.Add(index);
			StopWatch timer = StopWatch.StartNew();
			for (int32 index = 0; index < iterations; index++)
			{
				if (operation == 9) { list.Insert(width / 2, -1); list.RemoveAt(width / 2); }
				else { if (list.Remove(width - 1)) checksum++; list.Add(width - 1); }
			}
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return (checksum + list.Count) % 1000003;
		}
		if (operation == 11)
		{
			List<int32> list = new List<int32>(width);
			for (int32 index = 0; index < width; index++) list.Add(index);
			StopWatch timer = StopWatch.StartNew(); list.Clear(); timer.Stop();
			ElapsedTicks = timer.Elapsed.Ticks; return list.Count;
		}
		if (operation == 12)
		{
			List<int32> list = new List<int32>(width);
			for (int32 index = 0; index < width; index++) list.Add(index);
			StopWatch timer = StopWatch.StartNew();
			for (int32 iteration = 0; iteration < iterations; iteration++)
			{
				int32 sum = 0;
				for (let item in list) sum += item;
				checksum = (checksum + sum) % 1000003;
			}
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum;
		}
		if (operation == 47 || operation == 48 || operation == 49)
		{
			List<int32>?[] lists = new List<int32>?[iterations];
			for (int32 index = 0; index < iterations; index++)
			{
				List<int32> list = new List<int32>(width + 1);
				for (int32 item = 0; item < width; item++) list.Add(item);
				lists[index] = list;
			}
			StopWatch timer = StopWatch.StartNew();
			for (int32 index = 0; index < iterations; index++)
			{
				List<int32> list = lists[index]!;
				if (operation == 47) list.Insert(width / 2, -1);
				else if (operation == 48) list.RemoveAt(width / 2);
				else if (list.Remove(width - 1)) checksum++;
				checksum = (checksum + list.Count) % 1000003;
			}
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum;
		}
		if (operation == 13)
		{
			Stack<int32> stack = new Stack<int32>(iterations);
			StopWatch timer = StopWatch.StartNew();
			for (int32 index = 0; index < iterations; index++) stack.Push(index);
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return stack.Count;
		}
		if (operation == 14)
		{
			Stack<int32> stack = new Stack<int32>(iterations);
			for (int32 index = 0; index < iterations; index++) stack.Push(index);
			StopWatch timer = StopWatch.StartNew();
			while (stack.Count > 0) checksum = (checksum + stack.Pop()) % 1000003;
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum;
		}
		if (operation >= 15 && operation <= 16)
		{
			Stack<int32> stack = new Stack<int32>(width);
			for (int32 index = 0; index < width; index++) stack.Push(index);
			StopWatch timer = StopWatch.StartNew();
			for (int32 index = 0; index < iterations; index++)
				checksum = (checksum + (operation == 15 ? stack.Peek() : stack.Count)) % 1000003;
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum;
		}
		if (operation == 17)
		{
			Stack<int32> stack = new Stack<int32>(width);
			for (int32 index = 0; index < width; index++) stack.Push(index);
			StopWatch timer = StopWatch.StartNew(); stack.Clear(); timer.Stop();
			ElapsedTicks = timer.Elapsed.Ticks; return stack.Count;
		}
		if (operation == 18)
		{
			Stack<int32> stack = new Stack<int32>(width);
			for (int32 index = 0; index < width; index++) stack.Push(index);
			StopWatch timer = StopWatch.StartNew();
			for (int32 iteration = 0; iteration < iterations; iteration++)
			{
				int32 sum = 0;
				for (let item in stack) sum += item;
				checksum = (checksum + sum) % 1000003;
			}
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum;
		}
		if (operation == 19)
		{
			Queue<int32> queue = new Queue<int32>(iterations);
			StopWatch timer = StopWatch.StartNew();
			for (int32 index = 0; index < iterations; index++) queue.Enqueue(index);
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return queue.Count;
		}
		if (operation == 20)
		{
			Queue<int32> queue = new Queue<int32>(iterations);
			for (int32 index = 0; index < iterations; index++) queue.Enqueue(index);
			StopWatch timer = StopWatch.StartNew();
			while (queue.Count > 0) checksum = (checksum + queue.Dequeue()) % 1000003;
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum;
		}
		if (operation >= 21 && operation <= 22)
		{
			Queue<int32> queue = new Queue<int32>(width);
			for (int32 index = 0; index < width; index++) queue.Enqueue(index);
			StopWatch timer = StopWatch.StartNew();
			for (int32 index = 0; index < iterations; index++)
				checksum = (checksum + (operation == 21 ? queue.Peek() : queue.Count)) % 1000003;
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum;
		}
		if (operation == 23)
		{
			Queue<int32> queue = new Queue<int32>(width);
			for (int32 index = 0; index < width; index++) queue.Enqueue(index);
			StopWatch timer = StopWatch.StartNew(); queue.Clear(); timer.Stop();
			ElapsedTicks = timer.Elapsed.Ticks; return queue.Count;
		}
		if (operation == 24)
		{
			Queue<int32> queue = new Queue<int32>(width);
			for (int32 index = 0; index < width; index++) queue.Enqueue(index);
			StopWatch timer = StopWatch.StartNew();
			for (int32 iteration = 0; iteration < iterations; iteration++)
			{
				int32 sum = 0;
				for (let item in queue) sum += item;
				checksum = (checksum + sum) % 1000003;
			}
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum;
		}
		if (operation >= 25 && operation <= 32)
		{
			HashSet<int32> set = new HashSet<int32>(iterations > width ? iterations : width);
			int32 populateCount = operation == 29 ? iterations : width;
			if (operation == 25) populateCount = 0;
			if (operation != 25)
				for (int32 index = 0; index < populateCount; index++) set.Add(index);
			StopWatch timer = StopWatch.StartNew();
			if (operation == 25 || operation == 29 || operation == 30)
			{
				for (int32 index = 0; index < iterations; index++)
				{
					bool changed = operation == 25 ? set.Add(index) : operation == 29 ? set.Remove(index) : set.Remove(-index - 1);
					if (changed) checksum++;
					checksum %= 1000003;
				}
			}
			else if (operation == 26)
			{
				for (int32 index = 0; index < iterations; index++) if (!set.Add(index & (width - 1))) checksum++;
			}
			else if (operation == 27 || operation == 28)
			{
				for (int32 index = 0; index < iterations; index++)
					if (set.Contains(operation == 27 ? index & (width - 1) : -index - 1)) checksum++;
			}
			else if (operation == 31)
			{
				for (int32 index = 0; index < iterations; index++) checksum += set.Count;
			}
			else if (operation == 32) set.Clear();
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum % 1000003;
		}
		if (operation == 33)
		{
			HashSet<int32> set = new HashSet<int32>(width);
			for (int32 index = 0; index < width; index++) set.Add(index);
			StopWatch timer = StopWatch.StartNew();
			for (int32 iteration = 0; iteration < iterations; iteration++)
			{
				int32 sum = 0;
				for (let item in set) sum += item;
				checksum = (checksum + sum) % 1000003;
			}
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum;
		}
		if (operation >= 34 && operation <= 46)
		{
			Dictionary<int32, int32> dictionary = new Dictionary<int32, int32>(iterations > width ? iterations : width);
			int32 populateCount = operation == 43 ? iterations : width;
			if (operation == 34 || operation == 36) populateCount = 0;
			if (operation != 34 && operation != 36)
				for (int32 index = 0; index < populateCount; index++) dictionary.Add(index, index * 3);
			StopWatch timer = StopWatch.StartNew();
			int32 value = 0;
			if (operation == 34 || operation == 36)
			{
				for (int32 index = 0; index < iterations; index++)
				{
					if (operation == 34) { dictionary.Add(index, index * 3); checksum++; }
					else if (dictionary.TryAdd(index, index * 3)) checksum++;
				}
			}
			else if (operation == 35)
			{
				for (int32 index = 0; index < iterations; index++) if (!dictionary.TryAdd(index & (width - 1), 0)) checksum++;
			}
			else if (operation == 37 || operation == 38)
			{
				for (int32 index = 0; index < iterations; index++)
					if (dictionary.ContainsKey(operation == 37 ? index & (width - 1) : -index - 1)) checksum++;
			}
			else if (operation == 39 || operation == 40)
			{
				for (int32 index = 0; index < iterations; index++)
				{
					int32 key = operation == 39 ? index & (width - 1) : -index - 1;
					if (dictionary.TryGetValue(key, out value)) checksum += value;
				}
			}
			else if (operation == 41)
			{
				for (int32 index = 0; index < iterations; index++) checksum += dictionary[index & (width - 1)];
			}
			else if (operation == 42)
			{
				for (int32 index = 0; index < iterations; index++) dictionary[index & (width - 1)] = index;
				checksum = dictionary[0];
			}
			else if (operation == 43 || operation == 44)
			{
				for (int32 index = 0; index < iterations; index++)
					if (dictionary.Remove(operation == 43 ? index : -index - 1)) checksum++;
			}
			else if (operation == 45)
			{
				for (int32 index = 0; index < iterations; index++) checksum += dictionary.Count;
			}
			else if (operation == 46) dictionary.Clear();
			timer.Stop(); ElapsedTicks = timer.Elapsed.Ticks; return checksum % 1000003;
		}
		return -1;
	}
}
