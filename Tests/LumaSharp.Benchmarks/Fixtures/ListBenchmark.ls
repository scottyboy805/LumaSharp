using System.Collections;

public class ListBenchmark
{
	public static int32 Run(int32 iterations)
	{
		List<int32> defaultList = new List<int32>();
		List<int32> list = new List<int32>(4);
		int32 checksum = 0;

		for (int32 iteration = 0; iteration < iterations; iteration++)
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
		return checksum;
	}
}
