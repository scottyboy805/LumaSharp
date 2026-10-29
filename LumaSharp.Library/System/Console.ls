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
			IO.NativeFileSystem.luma_fs_console_write(null, 0, appendNewLine);
			return;
		}

		IO.NativeFileSystem.luma_fs_console_write(value.Ptr, value.Length, appendNewLine);
	}
}
