namespace System;

public struct UInt32 : ValueType
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

	public uint8[] GetBytes()
	{
		uint8[] bytes = new uint8[4];
		GetBytes(bytes);
		return bytes;
	}

	public void GetBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 4)
			throw new ArgumentException("The destination array is too short.");

		uint32 bits = (uint32)this;
		for (int32 index = 0; index < 4; index++)
			bytes[index] = (uint8)((bits >> (index * 8)) & 0xFF);
	}

	public static uint32 FromBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 4)
			throw new ArgumentException("The source array is too short.");

		uint32 bits = 0;
		for (int32 index = 0; index < 4; index++)
			bits |= (uint32)bytes[index] << (index * 8);

		return bits;
	}
}
