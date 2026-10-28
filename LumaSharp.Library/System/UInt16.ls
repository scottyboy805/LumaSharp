namespace System;

public static class UInt16
{
	public const uint16 MinValue = 0; public const uint16 MaxValue = 65535;
	public static uint16 Parse(string? text) { if (TryParse(text, out uint16 result)) return result; throw new FormatException("Invalid unsigned 16-bit integer."); }
	public static bool TryParse(string? text, out uint16 result) { result = 0; if (!UInt32.TryParse(text, out uint32 value) || value > MaxValue) return false; result = (uint16)value; return true; }
}

