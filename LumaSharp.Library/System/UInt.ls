namespace System;

public struct UInt : IHashable, IEquality<uint>
{
	public int32 Hash
	{
		get => HashCode.Calculate(this);
	}

	public bool Equals(uint other)
	{
		return this == other;
	}

	public const uint MinValue = 0;
	public const uint MaxValue = 0;
	public static uint Parse(string? text) { if (UInt64.TryParse(text, out uint64 value) && value <= MaxValue) return (uint)value; throw new FormatException("Invalid native unsigned integer."); }
	public static bool TryParse(string? text, out uint result) { result = 0; if (!UInt64.TryParse(text, out uint64 value) || value > MaxValue) return false; result = (uint)value; return true; }

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

		uint64 value = this;
		value.GetBytes(bytes);
	}

	public static uint FromBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 8)
			throw new ArgumentException("The source array is too short.");

		return (uint)uint64.FromBytes(bytes);
	}
}
