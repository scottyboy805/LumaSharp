using System;
using System.Collections;
using System.Diagnostics;

public class ListToArrayBenchmark
{
	public static int64 ElapsedTicks;

	public static int32 Run(int32 itemCount, int32 copies)
	{
		List<int32> list = new List<int32>();
		for (int32 index = 0; index < itemCount; index++)
			list.Add((index * 31 + 17) % 65521);

		if (list.Capacity <= itemCount)
			return -1;

		int32[] validation = list.ToArray();
		for (int32 index = 0; index < itemCount; index++)
		{
			if (validation[index] != (index * 31 + 17) % 65521)
				return -2;
		}

		int32 checksum = 0;
		StopWatch timer = StopWatch.StartNew();
		for (int32 iteration = 0; iteration < copies; iteration++)
		{
			int32[] array = list.ToArray();
			checksum = (checksum + array[0] + array[itemCount / 2] + array[itemCount - 1]) % 65521;
		}
		timer.Stop();
		ElapsedTicks = timer.Elapsed.Ticks;
		return checksum;
	}
}
