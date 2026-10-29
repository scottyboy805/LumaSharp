namespace System;

public struct UInt16 : ValueType
{
	public const uint16 MinValue = 0; public const uint16 MaxValue = 65535;
	public static uint16 Parse(string? text) { if (TryParse(text, out uint16 result)) return result; throw new FormatException("Invalid unsigned 16-bit integer."); }
	public static bool TryParse(string? text, out uint16 result) { result = 0; if (!UInt32.TryParse(text, out uint32 value) || value > MaxValue) return false; result = (uint16)value; return true; }

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

		uint16 bits = (uint16)this;
		for (int32 index = 0; index < 2; index++)
			bytes[index] = (uint8)((bits >> (index * 8)) & 0xFF);
	}

	public static uint16 FromBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 2)
			throw new ArgumentException("The source array is too short.");

		uint16 bits = 0;
		for (int32 index = 0; index < 2; index++)
			bits |= (uint16)bytes[index] << (index * 8);

		return bits;
	}
}
