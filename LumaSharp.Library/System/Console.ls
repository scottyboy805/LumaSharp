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

		int32 byteCount = 0;
		for (int32 index = 0; index < value.Length; index++)
		{
			char character = value[index];
			if (character <= '\u007F')
				byteCount++;
			else if (character <= '\u07FF')
				byteCount += 2;
			else if (character >= '\uD800' && character <= '\uDBFF' && index + 1 < value.Length && value[index + 1] >= '\uDC00' && value[index + 1] <= '\uDFFF')
			{
				byteCount += 4;
				index++;
			}
			else
				byteCount += 3;
		}

		IO.NativeFileSystem.luma_fs_console_write(value.Utf8Ptr, byteCount, appendNewLine);
	}
}
