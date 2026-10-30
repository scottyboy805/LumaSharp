namespace System;

public struct Int32 : ValueType, IHashable, IEquality<int32>
{
	public int32 Hash
	{
		get => HashCode.Calculate(this);
	}

	public bool Equals(int32 other)
	{
		return this == other;
	}

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

		for (int32 index = 0; index < 4; index++)
			bytes[index] = (uint8)((this >> (index * 8)) & 0xFF);
	}

	public static int32 FromBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 4)
			throw new ArgumentException("The source array is too short.");

		uint32 bits = 0;
		for (int32 index = 0; index < 4; index++)
			bits |= (uint32)bytes[index] << (index * 8);

		if (bits <= (uint32)MaxValue)
			return (int32)bits;

		return -1 - (int32)(UInt32.MaxValue - bits);
	}
}
