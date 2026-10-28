namespace System;

public static class String
{
	public static readonly string Empty = "";
	public static bool IsNullOrEmpty(string? value) => value == null || value.Length == 0;
	public static bool IsNullOrWhiteSpace(string? value)
	{
		if (value == null || value.Length == 0) return true;
		for (int32 i = 0; i < value.Length; i++)
			if (!(value[i] == ' ' || value[i] == '\t' || value[i] == '\n' || value[i] == '\r')) return false;
		return true;
	}
	public static string Concat(string? left, string? right) => (left ?? "") + (right ?? "");
}

