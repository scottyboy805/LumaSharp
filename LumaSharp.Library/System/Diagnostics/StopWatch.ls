using System;

namespace System.Diagnostics;

public class StopWatch
{
	private int64 startTimestamp;
	private int64 timestampFrequency;
	private int64 elapsedTimestampTicks;
	private bool isRunning;

	public this()
	{
		unsafe
		{
			timestampFrequency = NativeStopWatch.luma_diag_frequency();
		}
		elapsedTimestampTicks = 0;
		startTimestamp = 0;
		isRunning = false;
	}

	public bool IsRunning
	{
		get => isRunning;
	}

	public TimeSpan Elapsed
	{
		get
		{
			int64 timestampTicks = elapsedTimestampTicks;
			if (isRunning)
			{
				int64 timestamp;
				unsafe
				{
					timestamp = NativeStopWatch.luma_diag_timestamp();
				}
				timestampTicks += timestamp - startTimestamp;
			}
			int64 wholeSeconds = timestampTicks / timestampFrequency;
			int64 remainder = timestampTicks % timestampFrequency;
			int64 ticks = wholeSeconds * TimeSpan.TicksPerSecond + remainder * TimeSpan.TicksPerSecond / timestampFrequency;
			return TimeSpan(ticks);
		}
	}

	public static StopWatch StartNew()
	{
		StopWatch stopWatch = new StopWatch();
		stopWatch.Start();
		return stopWatch;
	}

	public void Start() mut
	{
		if (isRunning)
			return;

		unsafe
		{
			startTimestamp = NativeStopWatch.luma_diag_timestamp();
		}
		isRunning = true;
	}

	public void Stop() mut
	{
		if (!isRunning)
			return;

		int64 timestamp;
		unsafe
		{
			timestamp = NativeStopWatch.luma_diag_timestamp();
		}
		elapsedTimestampTicks += timestamp - startTimestamp;
		isRunning = false;
	}

	public void Reset() mut
	{
		startTimestamp = 0;
		elapsedTimestampTicks = 0;
		isRunning = false;
	}

	public void Restart() mut
	{
		elapsedTimestampTicks = 0;
		unsafe
		{
			startTimestamp = NativeStopWatch.luma_diag_timestamp();
		}
		isRunning = true;
	}
}

internal static unsafe class NativeStopWatch
{
	public static extern int64 luma_diag_timestamp();
	public static extern int64 luma_diag_frequency();
}
