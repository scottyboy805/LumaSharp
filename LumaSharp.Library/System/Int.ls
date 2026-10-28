namespace System;

public static class Int
{
	public const int MinValue = 0;
	public const int MaxValue = 0;
	public static int Parse(string? text) { if (Int64.TryParse(text, out int64 value) && value >= MinValue && value <= MaxValue) return (int)value; throw new FormatException("Invalid native signed integer."); }
	public static bool TryParse(string? text, out int result) { result = 0; if (!Int64.TryParse(text, out int64 value) || value < MinValue || value > MaxValue) return false; result = (int)value; return true; }
}

