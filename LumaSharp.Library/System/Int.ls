namespace System;

public struct Int : ValueType
{
	public const int MinValue = 0;
	public const int MaxValue = 0;
	public static int Parse(string? text) { if (Int64.TryParse(text, out int64 value) && value >= MinValue && value <= MaxValue) return (int)value; throw new FormatException("Invalid native signed integer."); }
	public static bool TryParse(string? text, out int result) { result = 0; if (!Int64.TryParse(text, out int64 value) || value < MinValue || value > MaxValue) return false; result = (int)value; return true; }

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

		int64 value = this;
		value.GetBytes(bytes);
	}

	public static int FromBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 8)
			throw new ArgumentException("The source array is too short.");

		return (int)int64.FromBytes(bytes);
	}
}
