namespace System;

public struct Char : IHashable, IEquality<char>
{
	public int32 Hash
	{
		get => HashCode.Calculate(this);
	}

	public bool Equals(char other)
	{
		return this == other;
	}

	public const char MinValue = '\0'; public const char MaxValue = '\uFFFF';
	public static char Parse(string? text) { if (TryParse(text, out char result)) return result; throw new FormatException("Input string must contain exactly one character."); }
	public static bool TryParse(string? text, out char result) { result = '\0'; if (text == null || text.Length != 1) return false; result = text[0]; return true; }

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
		bytes[0] = (uint8)(bits & 0xFF);
		bytes[1] = (uint8)((bits >> 8) & 0xFF);
	}

	public static char FromBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 2)
			throw new ArgumentException("The source array is too short.");

		uint16 bits = (uint16)bytes[0];
		bits |= (uint16)bytes[1] << 8;
		return (char)bits;
	}
}
