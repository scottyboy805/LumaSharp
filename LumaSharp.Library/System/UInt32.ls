namespace System;

public static class UInt32
{
	public const uint32 MinValue = 0;
	public const uint32 MaxValue = 4294967295u;
	public static uint32 Parse(string? text)
	{
		if (TryParse(text, out uint32 result)) return result;
		throw new FormatException("Input string was not a valid unsigned 32-bit integer.");
	}
	public static bool TryParse(string? text, out uint32 result)
	{
		result = 0;
		if (text == null || text.Length == 0) return false;
		int32 index = 0;
		if (text[0] == '+') index = 1;
		if (index == text.Length) return false;
		uint32 value = 0;
		while (index < text.Length)
		{
			char character = text[index++];
			if (character < '0' || character > '9') return false;
			uint32 digit = (uint32)(character - '0');
			if (value > (MaxValue - digit) / 10u) return false;
			value = value * 10u + digit;
		}
		result = value;
		return true;
	}
}

