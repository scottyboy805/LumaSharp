namespace System;

public static class UInt64
{
	public const uint64 MinValue = 0; public const uint64 MaxValue = 18446744073709551615ul;
	public static uint64 Parse(string? text) { if (TryParse(text, out uint64 result)) return result; throw new FormatException("Invalid unsigned 64-bit integer."); }
	public static bool TryParse(string? text, out uint64 result)
	{
		result = 0; if (text == null || text.Length == 0) return false;
		int32 index = 0; if (text[0] == '+') index = 1; if (index == text.Length) return false;
		uint64 value = 0;
		while (index < text.Length) { char character = text[index++]; if (character < '0' || character > '9') return false; uint64 digit = (uint64)(character - '0'); if (value > (MaxValue - digit) / 10ul) return false; value = value * 10ul + digit; }
		result = value; return true;
	}
}

