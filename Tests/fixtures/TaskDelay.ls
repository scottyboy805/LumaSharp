using System;
using System.Diagnostics;
using System.Threading;

public class Program
{
	private static async int32 MeasureDelayAsync()
	{
		StopWatch watch = StopWatch.StartNew();
		Task pending = Task.Delay(40);
		if (pending.IsCompleted)
			return 1;

		await (pending);
		if (!pending.IsCompleted || watch.Elapsed.TotalMilliseconds < 25.0)
			return 2;

		watch.Restart();
		await Task.Delay(TimeSpan(50 * TimeSpan.TicksPerMillisecond));
		if (watch.Elapsed.TotalMilliseconds < 35.0)
			return 3;

		watch.Restart();
		Task firstWait = Task.Delay(30);
		Task secondWait = Task.Delay(60);
		await (firstWait);
		await (secondWait);
		if (watch.Elapsed.TotalMilliseconds < 45.0)
			return 8;

		return 0;
	}

	public static int32 Main()
	{
		if (!Task.Delay(0).IsCompleted || !Task.Delay(TimeSpan(0)).IsCompleted)
			return 4;
		if (Task.Delay(-1).IsCompleted || Task.Delay(TimeSpan(-TimeSpan.TicksPerMillisecond)).IsCompleted)
			return 7;

		try
		{
			Task.Delay(-2);
			return 5;
		}
		catch (ArgumentOutOfRangeException)
		{
		}

		try
		{
			Task.Delay(TimeSpan(-2 * TimeSpan.TicksPerMillisecond));
			return 6;
		}
		catch (ArgumentOutOfRangeException)
		{
		}

		return MeasureDelayAsync().Result;
	}
}
