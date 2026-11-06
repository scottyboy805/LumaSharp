using System;
using System.Collections;

public class CompilerBuildBenchmark
{
	public static int32 Main()
	{
		List<int32> values = new List<int32>();
		values.Add(17);
		values.Add(25);
		int32 sum = values[0] + values[1];
		Console.WriteLine(sum.ToString());
		return 0;
	}
}
