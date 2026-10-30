namespace System;

public struct UInt8 : IHashable, IEquality<uint8>
{
	public int32 Hash
	{
		get => HashCode.Calculate(this);
	}

	public bool Equals(uint8 other)
	{
		return this == other;
	}

	public const uint8 MinValue = 0; public const uint8 MaxValue = 255;
	public static uint8 Parse(string? text) { if (TryParse(text, out uint8 result)) return result; throw new FormatException("Invalid unsigned 8-bit integer."); }
	public static bool TryParse(string? text, out uint8 result) { result = 0; if (!UInt32.TryParse(text, out uint32 value) || value > MaxValue) return false; result = (uint8)value; return true; }

	public uint8[] GetBytes()
	{
		uint8[] bytes = new uint8[1];
		GetBytes(bytes);
		return bytes;
	}

	public void GetBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 1)
			throw new ArgumentException("The destination array is too short.");

		uint8 bits = (uint8)this;
		for (int32 index = 0; index < 1; index++)
			bytes[index] = (uint8)((bits >> (index * 8)) & 0xFF);
	}

	public static uint8 FromBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 1)
			throw new ArgumentException("The source array is too short.");

		uint8 bits = 0;
		for (int32 index = 0; index < 1; index++)
			bits |= (uint8)bytes[index] << (index * 8);

		return bits;
	}
}
