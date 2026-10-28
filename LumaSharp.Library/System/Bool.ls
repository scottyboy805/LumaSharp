namespace System;

public static class Bool
{
	public const string TrueString = "True";
	public const string FalseString = "False";
	public static bool Parse(string value)
	{
		if (EqualsIgnoreCase(value, TrueString)) return true;
		if (EqualsIgnoreCase(value, FalseString)) return false;
		throw new FormatException("Input string was not a valid boolean.");
	}
	public static bool TryParse(string? value, out bool result)
	{
		result = false;
		if (value == null) return false;
		if (EqualsIgnoreCase(value, TrueString)) { result = true; return true; }
		if (EqualsIgnoreCase(value, FalseString)) return true;
		return false;
	}
	private static bool EqualsIgnoreCase(string value, string expected)
	{
		if (value.Length != expected.Length) return false;
		for (int32 i = 0; i < value.Length; i++)
		{
			char actual = value[i];
			if (actual >= 'a' && actual <= 'z') actual = (char)(actual - 'a' + 'A');
			if (actual != expected[i]) return false;
		}
		return true;
	}
}

