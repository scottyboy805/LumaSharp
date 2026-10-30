namespace System;

public struct Int16 : IHashable, IEquality<int16>
{
	public int32 Hash
	{
		get => HashCode.Calculate(this);
	}

	public bool Equals(int16 other)
	{
		return this == other;
	}

	public const int16 MinValue = -32768; public const int16 MaxValue = 32767;
	public static int16 Parse(string? text) { if (TryParse(text, out int16 result)) return result; throw new FormatException("Invalid 16-bit integer."); }
	public static bool TryParse(string? text, out int16 result) { result = 0; if (!Int32.TryParse(text, out int32 value) || value < MinValue || value > MaxValue) return false; result = (int16)value; return true; }

	public uint8[] GetBytes()
	{
		uint8[] bytes = new uint8[2];
		GetBytes(bytes);
		return bytes;
	}

	public void GetBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 2)
			throw new ArgumentException("The destination array is too short.");

		for (int32 index = 0; index < 2; index++)
			bytes[index] = (uint8)((this >> (index * 8)) & 0xFF);
	}

	public static int16 FromBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 2)
			throw new ArgumentException("The source array is too short.");

		uint16 bits = 0;
		for (int32 index = 0; index < 2; index++)
			bits |= (uint16)bytes[index] << (index * 8);

		if (bits <= (uint16)MaxValue)
			return (int16)bits;

		return (int16)(-1 - (int16)(UInt16.MaxValue - bits));
	}
}
