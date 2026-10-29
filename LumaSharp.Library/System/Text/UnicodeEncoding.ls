namespace System.Text;

public class UnicodeEncoding : Encoding
{
	public override uint8[] GetBytes(ReadOnlySpan<char> chars)
	{
		uint8[] bytes = new uint8[chars.Length * 2];
		GetBytes(chars, bytes);
		return bytes;
	}

	public void GetBytes(ReadOnlySpan<char> chars, scope uint8[] bytes)
	{
		int32 requiredLength = chars.Length * 2;
		if (bytes.Length < requiredLength)
			throw new System.ArgumentException("The destination array is too short.");

		for (int32 index = 0; index < chars.Length; index++)
		{
			uint16 value = (uint16)chars[index];
			int32 byteIndex = index * 2;
			bytes[byteIndex] = (uint8)(value & 0xFF);
			bytes[byteIndex + 1] = (uint8)((value >> 8) & 0xFF);
		}
	}

	public override int32 GetCharCount(scope uint8[] bytes)
	{

		return (bytes.Length + 1) / 2;
	}

	public override char[] GetChars(scope uint8[] bytes)
	{

		int32 charCount = GetCharCount(bytes);
		char[] chars = new char[charCount];
		for (int32 index = 0; index < charCount; index++)
		{
			int32 byteIndex = index * 2;
			uint16 value = (uint16)bytes[byteIndex];
			if (byteIndex + 1 < bytes.Length)
				value |= (uint16)bytes[byteIndex + 1] << 8;
			else
				value = 65533;

			chars[index] = (char)value;
		}

		return chars;
	}
}
