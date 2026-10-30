namespace System;

public struct Float : IHashable, IEquality<float>
{
	public int32 Hash
	{
		get => HashCode.Calculate(this);
	}

	public bool Equals(float other)
	{
		if (IsNaN(this))
			return IsNaN(other);
		return this == other;
	}

	public const float MinValue = -3.40282347e38f; public const float MaxValue = 3.40282347e38f;
	public const float Epsilon = 1.40129846e-45f;
	public static float NaN { get => 0.0f / 0.0f; }
	public static float PositiveInfinity { get => 1.0f / 0.0f; }
	public static float NegativeInfinity { get => -1.0f / 0.0f; }
	public static bool IsNaN(float value) => value != value;
	public static bool IsInfinity(float value) => value == PositiveInfinity || value == NegativeInfinity;
	public static float Parse(string? text) { if (TryParse(text, out float result)) return result; throw new FormatException("Invalid single-precision number."); }
	public static bool TryParse(string? text, out float result) { result = 0.0f; if (!Double.TryParse(text, out double value)) return false; result = (float)value; return true; }

	public unsafe uint8[] GetBytes()
	{
		uint8[] bytes = new uint8[4];
		GetBytes(bytes);
		return bytes;
	}

	public unsafe void GetBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 4)
			throw new ArgumentException("The destination array is too short.");

		uint32 bits = ToBits(this);
		for (int32 index = 0; index < 4; index++)
			bytes[index] = (uint8)((bits >> (index * 8)) & 0xFF);
	}

	public static unsafe float FromBytes(scope uint8[] bytes)
	{
		if (bytes.Length < 4)
			throw new ArgumentException("The source array is too short.");

		uint32 bits = 0;
		for (int32 index = 0; index < 4; index++)
			bits |= (uint32)bytes[index] << (index * 8);

		return FromBits(bits);
	}

	private static unsafe uint32 ToBits(float value)
	{
		uint32* bits = (uint32*)&value;
		return *bits;
	}

	private static unsafe float FromBits(uint32 bits)
	{
		float value = 0.0f;
		uint32* destination = (uint32*)&value;
		*destination = bits;
		return value;
	}
}
