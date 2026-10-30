namespace System;

public struct TimeSpan : ValueType
{
	public const int64 TicksPerMillisecond = 10000;
	public const int64 TicksPerSecond = 10000000;
	public const int64 TicksPerMinute = 600000000;
	public const int64 TicksPerHour = 36000000000;
	public const int64 TicksPerDay = 864000000000;

	private int64 ticks;

	public this(int64 ticks)
	{
		this.ticks = ticks;
	}

	public int64 Ticks
	{
		get => ticks;
	}

	public double TotalMilliseconds
	{
		get => (double)ticks / (double)TicksPerMillisecond;
	}

	public double TotalSeconds
	{
		get => (double)ticks / (double)TicksPerSecond;
	}
}
