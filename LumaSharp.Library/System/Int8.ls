namespace System;

public struct Int8 : ValueType, IHashable, IEquality<int8>
{
	public int32 Hash
	{
		get => HashCode.Calculate(this);
	}

	public bool Equals(int8 other)
	{
		return this == other;
	}

	public const int8 MinValue = -128; public const int8 MaxValue = 127;
	public static int8 Parse(string? text) { if (TryParse(text, out int8 result)) return result; throw new FormatException("Invalid 8-bit integer."); }
	public static bool TryParse(string? text, out int8 result) { result = 0; if (!Int32.TryParse(text, out int32 value) || value < MinValue || value > MaxValue) return false; result = (int8)value; return true; }

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

		bytes[0] = (uint8)(this & 0xFF);
	}

	public static int8 FromBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 1)
			throw new ArgumentException("The source array is too short.");

		if (bytes[0] <= MaxValue)
			return (int8)bytes[0];

		return (int8)(-1 - (int8)(UInt8.MaxValue - bytes[0]));
	}
}
