namespace System;

public struct Float : ValueType
{
	public const float MinValue = -3.40282347e38f; public const float MaxValue = 3.40282347e38f;
	public const float Epsilon = 1.40129846e-45f;
	public static float NaN { get => 0.0f / 0.0f; }
	public static float PositiveInfinity { get => 1.0f / 0.0f; }
	public static float NegativeInfinity { get => -1.0f / 0.0f; }
	public static bool IsNaN(float value) => value != value;
	public static bool IsInfinity(float value) => value == PositiveInfinity || value == NegativeInfinity;
	public static float Parse(string? text) { if (TryParse(text, out float result)) return result; throw new FormatException("Invalid single-precision number."); }
	public static bool TryParse(string? text, out float result) { result = 0.0f; if (!Double.TryParse(text, out double value)) return false; result = (float)value; return true; }
}

