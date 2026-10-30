namespace System;

public struct UInt64 : IHashable, IEquality<uint64>
{
	public int32 Hash
	{
		get => HashCode.Calculate(this);
	}

	public bool Equals(uint64 other)
	{
		return this == other;
	}

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

	public uint8[] GetBytes()
	{
		uint8[] bytes = new uint8[8];
		GetBytes(bytes);
		return bytes;
	}

	public void GetBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 8)
			throw new ArgumentException("The destination array is too short.");

		uint64 bits = (uint64)this;
		for (int32 index = 0; index < 8; index++)
			bytes[index] = (uint8)((bits >> (index * 8)) & 0xFF);
	}

	public static uint64 FromBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 8)
			throw new ArgumentException("The source array is too short.");

		uint64 bits = 0;
		for (int32 index = 0; index < 8; index++)
			bits |= (uint64)bytes[index] << (index * 8);

		return bits;
	}
}
