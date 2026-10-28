namespace System;

public struct Int8 : ValueType
{
	public const int8 MinValue = -128; public const int8 MaxValue = 127;
	public static int8 Parse(string? text) { if (TryParse(text, out int8 result)) return result; throw new FormatException("Invalid 8-bit integer."); }
	public static bool TryParse(string? text, out int8 result) { result = 0; if (!Int32.TryParse(text, out int32 value) || value < MinValue || value > MaxValue) return false; result = (int8)value; return true; }
}

