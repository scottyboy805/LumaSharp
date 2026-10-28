namespace System;

public struct UInt : ValueType
{
	public const uint MinValue = 0;
	public const uint MaxValue = 0;
	public static uint Parse(string? text) { if (UInt64.TryParse(text, out uint64 value) && value <= MaxValue) return (uint)value; throw new FormatException("Invalid native unsigned integer."); }
	public static bool TryParse(string? text, out uint result) { result = 0; if (!UInt64.TryParse(text, out uint64 value) || value > MaxValue) return false; result = (uint)value; return true; }
}

