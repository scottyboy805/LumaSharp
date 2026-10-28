namespace System;

public static class Double
{
	public const double MinValue = -1.7976931348623157e308; public const double MaxValue = 1.7976931348623157e308;
	public const double Epsilon = 4.9406564584124654e-324;
	public static double NaN { get => 0.0 / 0.0; }
	public static double PositiveInfinity { get => 1.0 / 0.0; }
	public static double NegativeInfinity { get => -1.0 / 0.0; }
	public static bool IsNaN(double value) => value != value;
	public static bool IsInfinity(double value) => value == PositiveInfinity || value == NegativeInfinity;
	public static double Parse(string? text) { if (TryParse(text, out double result)) return result; throw new FormatException("Invalid double-precision number."); }
	public static bool TryParse(string? text, out double result)
	{
		result = 0.0; if (text == null || text.Length == 0) return false;
		int32 index = 0; bool negative = false;
		if (text[0] == '-' || text[0] == '+') { negative = text[0] == '-'; index = 1; }
		double value = 0.0; double fraction = 0.1; bool decimalPoint = false; bool hasDigit = false;
		while (index < text.Length && text[index] != 'e' && text[index] != 'E')
		{
			char character = text[index++];
			if (character == '.' && !decimalPoint) { decimalPoint = true; continue; }
			if (character < '0' || character > '9') return false;
			hasDigit = true; double digit = (double)(character - '0');
			if (decimalPoint) { value += digit * fraction; fraction *= 0.1; } else value = value * 10.0 + digit;
		}
		if (!hasDigit) return false;
		if (index < text.Length)
		{
			index++; bool exponentNegative = false;
			if (index < text.Length && (text[index] == '-' || text[index] == '+')) { exponentNegative = text[index] == '-'; index++; }
			if (index == text.Length) return false;
			int32 exponent = 0;
			while (index < text.Length) { char character = text[index++]; if (character < '0' || character > '9') return false; if (exponent < 400) exponent = exponent * 10 + (int32)(character - '0'); }
			if (exponent > 400) exponent = 400;
			for (int32 i = 0; i < exponent; i++) value = exponentNegative ? value / 10.0 : value * 10.0;
		}
		result = negative ? -value : value; return true;
	}
}

