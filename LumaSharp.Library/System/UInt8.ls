namespace System;

public static class UInt8
{
	public const uint8 MinValue = 0; public const uint8 MaxValue = 255;
	public static uint8 Parse(string? text) { if (TryParse(text, out uint8 result)) return result; throw new FormatException("Invalid unsigned 8-bit integer."); }
	public static bool TryParse(string? text, out uint8 result) { result = 0; if (!UInt32.TryParse(text, out uint32 value) || value > MaxValue) return false; result = (uint8)value; return true; }
}

