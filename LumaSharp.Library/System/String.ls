using System.Text;

namespace System;

public class String : Object
{
	public static unsafe implicit operator ReadOnlySpan<char>(String value)
	{
		if (value.Length == 0)
			return ReadOnlySpan<char>.Empty;

		return ReadOnlySpan<char>(value.Ptr, value.Length, value);
	}

	public unsafe char* Ptr
	{
		get => null;
	}

	public int32 Length
	{
		get => 0;
	}

	public char this[int32 index]
	{
		get => '\0';
	}

	public this(char[] value)
	{
	}

	public this(char[] value, int32 startIndex, int32 length)
	{
	}

	public this(char value, int32 repeatCount)
	{
	}

	public string Substring(int32 startIndex)
	{
		return Substring(startIndex, Length - startIndex);
	}

	public string Substring(int32 startIndex, int32 length)
	{
		if (startIndex < 0 || startIndex > Length || length < 0 || length > Length - startIndex)
			throw new ArgumentOutOfRangeException();

		string result = "";
		for (int32 index = 0; index < length; index++)
			result += this[startIndex + index].ToString();

		return result;
	}

	public int32 IndexOf(char value)
	{
		return IndexOf(value, 0);
	}

	public int32 IndexOf(char value, int32 startIndex)
	{
		if (startIndex < 0 || startIndex > Length)
			throw new ArgumentOutOfRangeException();

		for (int32 index = startIndex; index < Length; index++)
			if (this[index] == value)
				return index;

		return -1;
	}

	public int32 IndexOf(string? value)
	{
		return IndexOf(value, 0);
	}

	public int32 IndexOf(string? value, int32 startIndex)
	{
		if (value == null)
			throw new ArgumentNullException();
		if (startIndex < 0 || startIndex > Length)
			throw new ArgumentOutOfRangeException();
		if (value.Length == 0)
			return startIndex;

		int32 lastStart = Length - value.Length;
		for (int32 index = startIndex; index <= lastStart; index++)
		{
			bool matches = true;
			for (int32 offset = 0; offset < value.Length; offset++)
			{
				if (this[index + offset] != value[offset])
				{
					matches = false;
					break;
				}
			}
			if (matches)
				return index;
		}

		return -1;
	}

	public bool Contains(string value)
	{
		return IndexOf(value) >= 0;
	}

	public bool StartsWith(string? value)
	{
		return value != null && value.Length <= Length && IndexOf(value) == 0;
	}

	public bool EndsWith(string? value)
	{
		return value != null && value.Length <= Length && IndexOf(value, Length - value.Length) == Length - value.Length;
	}

	public string Replace(char oldValue, char newValue)
	{
		string result = "";
		for (int32 index = 0; index < Length; index++)
		{
			char current = this[index];
			result += (current == oldValue ? newValue : current).ToString();
		}
		return result;
	}

	public string Trim()
	{
		int32 start = 0;
		int32 end = Length;
		while (start < end && IsWhitespace(this[start]))
			start++;
		while (end > start && IsWhitespace(this[end - 1]))
			end--;
		return Substring(start, end - start);
	}

	public char[] ToCharArray()
	{
		char[] result = new char[Length];
		for (int32 index = 0; index < Length; index++)
			result[index] = this[index];
		return result;
	}

	public uint8[] GetBytes()
	{
		return GetBytes(Encoding.UTF8);
	}

	public uint8[] GetBytes(Encoding encoding)
	{
		return encoding.GetBytes((ReadOnlySpan<char>)this);
	}

	public void GetBytes(scope uint8[] bytes)
	{
		GetBytes(bytes, Encoding.UTF8);
	}

	public void GetBytes(scope uint8[] bytes, Encoding encoding)
	{
		uint8[] encoded = GetBytes(encoding);
		if (bytes.Length < encoded.Length)
			throw new ArgumentException("The destination array is too short.");

		for (int32 index = 0; index < encoded.Length; index++)
			bytes[index] = encoded[index];
	}

	public static string FromBytes(scope uint8[] bytes)
	{
		return FromBytes(bytes, Encoding.UTF8);
	}

	public static string FromBytes(scope uint8[] bytes, Encoding encoding)
	{
		return FromChars(encoding.GetChars(bytes));
	}

	private static bool IsWhitespace(char value)
	{
		return value == ' ' || value == '\t' || value == '\n' || value == '\r' || value == '\f' || value == '\v';
	}

	public static readonly string Empty = "";

	public static bool IsNullOrEmpty(string? value)
	{
		return value == null || value.Length == 0;
	}

	public static bool IsNullOrWhiteSpace(string? value)
	{
		if (value == null || value.Length == 0)
			return true;
		for (int32 index = 0; index < value.Length; index++)
			if (!IsWhitespace(value[index]))
				return false;
		return true;
	}

	public static string Concat(string? left, string? right)
	{
		return (left ?? "") + (right ?? "");
	}

	public static string FromChars(char[]? value)
	{
		if (value == null)
			throw new ArgumentNullException();
		return FromChars(value, 0, value.Length);
	}

	public static string FromChars(char[]? value, int32 startIndex, int32 length)
	{
		if (value == null)
			throw new ArgumentNullException();
		if (startIndex < 0 || startIndex > value.Length || length < 0 || length > value.Length - startIndex)
			throw new ArgumentOutOfRangeException();

		string result = "";
		for (int32 index = 0; index < length; index++)
			result += value[startIndex + index].ToString();
		return result;
	}

	public static string Repeat(char value, int32 repeatCount)
	{
		if (repeatCount < 0)
			throw new ArgumentOutOfRangeException();

		string result = "";
		string character = value.ToString();
		for (int32 index = 0; index < repeatCount; index++)
			result += character;
		return result;
	}
}

