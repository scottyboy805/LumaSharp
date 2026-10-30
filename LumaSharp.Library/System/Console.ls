namespace System;

public static unsafe class Console
{
	public static void Write(string? value)
	{
		WriteValue(value, false);
	}

	public static void Write(char value)
	{
		Write(value.ToString());
	}

	public static void WriteLine()
	{
		WriteValue(null, true);
	}

	public static void WriteLine(string? value)
	{
		WriteValue(value, true);
	}

	public static void WriteLine(char value)
	{
		WriteLine(value.ToString());
	}

	private static void WriteValue(string? value, bool appendNewLine)
	{
		if (value == null)
		{
			__conwrite(value, 0, appendNewLine);
			return;
		}

		__conwrite(value, value.Length, appendNewLine);
	}
}
