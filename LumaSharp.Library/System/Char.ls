namespace System;

public struct Char : ValueType
{
	public const char MinValue = '\0'; public const char MaxValue = '\uFFFF';
	public static char Parse(string? text) { if (TryParse(text, out char result)) return result; throw new FormatException("Input string must contain exactly one character."); }
	public static bool TryParse(string? text, out char result) { result = '\0'; if (text == null || text.Length != 1) return false; result = text[0]; return true; }
}

