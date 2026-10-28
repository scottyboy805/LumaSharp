namespace System;

public static class Int16
{
	public const int16 MinValue = -32768; public const int16 MaxValue = 32767;
	public static int16 Parse(string? text) { if (TryParse(text, out int16 result)) return result; throw new FormatException("Invalid 16-bit integer."); }
	public static bool TryParse(string? text, out int16 result) { result = 0; if (!Int32.TryParse(text, out int32 value) || value < MinValue || value > MaxValue) return false; result = (int16)value; return true; }
}

