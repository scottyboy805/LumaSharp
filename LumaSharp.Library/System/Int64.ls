namespace System;

public struct Int64 : ValueType
{
	public const int64 MinValue = -9223372036854775807L - 1; public const int64 MaxValue = 9223372036854775807L;
	public static int64 Parse(string? text) { if (TryParse(text, out int64 result)) return result; throw new FormatException("Invalid 64-bit integer."); }
	public static bool TryParse(string? text, out int64 result)
	{
		result = 0; if (text == null || text.Length == 0) return false;
		int32 index = 0; bool negative = false;
		if (text[0] == '-' || text[0] == '+') { negative = text[0] == '-'; index = 1; }
		if (index == text.Length) return false;
		int64 limit = negative ? MinValue : -MaxValue; int64 multiplyLimit = limit / 10L; int64 value = 0;
		while (index < text.Length) { char character = text[index++]; if (character < '0' || character > '9') return false; int64 digit = (int64)(character - '0'); if (value < multiplyLimit) return false; value *= 10L; if (value < limit + digit) return false; value -= digit; }
		result = negative ? value : -value; return true;
	}
}

