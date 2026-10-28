namespace System;

public struct Int32 : ValueType
{
	public const int32 MinValue = -2147483647 - 1;
	public const int32 MaxValue = 2147483647;
	public static int32 Parse(string? text)
	{
		if (TryParse(text, out int32 result)) return result;
		throw new FormatException("Input string was not a valid 32-bit integer.");
	}
	public static bool TryParse(string? text, out int32 result)
	{
		result = 0;
		if (text == null || text.Length == 0) return false;
		int32 index = 0;
		bool negative = false;
		if (text[0] == '-' || text[0] == '+') { negative = text[0] == '-'; index = 1; }
		if (index == text.Length) return false;
		int32 limit = negative ? MinValue : -MaxValue;
		int32 multiplyLimit = limit / 10;
		int32 value = 0;
		while (index < text.Length)
		{
			char character = text[index++];
			if (character < '0' || character > '9') return false;
			int32 digit = (int32)(character - '0');
			if (value < multiplyLimit) return false;
			value *= 10;
			if (value < limit + digit) return false;
			value -= digit;
		}
		result = negative ? value : -value;
		return true;
	}
}

