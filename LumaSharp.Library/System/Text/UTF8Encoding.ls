namespace System.Text;

public class UTF8Encoding : Encoding
{
	private const uint32 ReplacementCharacter = 65533;

	public override uint8[] GetBytes(ReadOnlySpan<char> chars)
	{
		int32 byteCount = 0;
		int32 index = 0;
		while (index < chars.Length)
		{
			uint32 scalar = GetScalar(chars, index, out int32 consumed);
			if (scalar <= 127)
				byteCount += 1;
			else if (scalar <= 2047)
				byteCount += 2;
			else if (scalar <= 65535)
				byteCount += 3;
			else
				byteCount += 4;
			index += consumed;
		}

		uint8[] bytes = new uint8[byteCount];
		GetBytes(chars, bytes);
		return bytes;
	}

	public void GetBytes(ReadOnlySpan<char> chars, scope uint8[] bytes)
	{

		int32 byteCount = GetByteCount(chars);
		if (bytes.Length < byteCount)
			throw new System.ArgumentException("The destination array is too short.");

		int32 charIndex = 0;
		int32 byteIndex = 0;
		while (charIndex < chars.Length)
		{
			uint32 scalar = GetScalar(chars, charIndex, out int32 consumed);
			if (scalar <= 127)
			{
				bytes[byteIndex] = (uint8)scalar;
				byteIndex++;
			}
			else if (scalar <= 2047)
			{
				bytes[byteIndex] = (uint8)(192 | (scalar >> 6));
				bytes[byteIndex + 1] = (uint8)(128 | (scalar & 63));
				byteIndex += 2;
			}
			else if (scalar <= 65535)
			{
				bytes[byteIndex] = (uint8)(224 | (scalar >> 12));
				bytes[byteIndex + 1] = (uint8)(128 | ((scalar >> 6) & 63));
				bytes[byteIndex + 2] = (uint8)(128 | (scalar & 63));
				byteIndex += 3;
			}
			else
			{
				bytes[byteIndex] = (uint8)(240 | (scalar >> 18));
				bytes[byteIndex + 1] = (uint8)(128 | ((scalar >> 12) & 63));
				bytes[byteIndex + 2] = (uint8)(128 | ((scalar >> 6) & 63));
				bytes[byteIndex + 3] = (uint8)(128 | (scalar & 63));
				byteIndex += 4;
			}

			charIndex += consumed;
		}
	}

	public override int32 GetCharCount(scope uint8[] bytes)
	{

		int32 charCount = 0;
		int32 index = 0;
		while (index < bytes.Length)
		{
			uint32 scalar;
			int32 consumed = DecodeNext(bytes, index, out scalar);
			charCount += scalar > 65535 ? 2 : 1;
			index += consumed;
		}

		return charCount;
	}

	public override char[] GetChars(scope uint8[] bytes)
	{

		char[] chars = new char[GetCharCount(bytes)];
		int32 byteIndex = 0;
		int32 charIndex = 0;
		while (byteIndex < bytes.Length)
		{
			uint32 scalar;
			int32 consumed = DecodeNext(bytes, byteIndex, out scalar);
			if (scalar <= 65535)
			{
				chars[charIndex] = (char)scalar;
				charIndex++;
			}
			else
			{
				uint32 supplementary = scalar - 65536;
				chars[charIndex] = (char)(55296 + (supplementary >> 10));
				chars[charIndex + 1] = (char)(56320 + (supplementary & 1023));
				charIndex += 2;
			}

			byteIndex += consumed;
		}

		return chars;
	}

	private int32 GetByteCount(ReadOnlySpan<char> chars)
	{
		int32 byteCount = 0;
		int32 index = 0;
		while (index < chars.Length)
		{
			uint32 scalar = GetScalar(chars, index, out int32 consumed);
			if (scalar <= 127)
				byteCount += 1;
			else if (scalar <= 2047)
				byteCount += 2;
			else if (scalar <= 65535)
				byteCount += 3;
			else
				byteCount += 4;
			index += consumed;
		}
		return byteCount;
	}

	private static uint32 GetScalar(ReadOnlySpan<char> chars, int32 index, out int32 consumed)
	{
		uint32 current = (uint32)chars[index];
		if (current >= 55296 && current <= 56319 && index + 1 < chars.Length)
		{
			uint32 next = (uint32)chars[index + 1];
			if (next >= 56320 && next <= 57343)
			{
				consumed = 2;
				return 65536 + ((current - 55296) << 10) + (next - 56320);
			}
		}

		consumed = 1;
		if (current >= 55296 && current <= 57343)
			return ReplacementCharacter;

		return current;
	}

	private static int32 DecodeNext(scope uint8[] bytes, int32 index, out uint32 scalar)
	{
		uint32 first = (uint32)bytes[index];
		if (first <= 127)
		{
			scalar = first;
			return 1;
		}

		if (first >= 194 && first <= 223 && index + 1 < bytes.Length)
		{
			uint32 second = (uint32)bytes[index + 1];
			if (IsContinuation(second))
			{
				scalar = ((first & 31) << 6) | (second & 63);
				return 2;
			}
		}

		if (first >= 224 && first <= 239 && index + 2 < bytes.Length)
		{
			uint32 second = (uint32)bytes[index + 1];
			uint32 third = (uint32)bytes[index + 2];
			bool validSecond = IsContinuation(second) &&
				(first != 224 || second >= 160) &&
				(first != 237 || second < 160);
			if (validSecond && IsContinuation(third))
			{
				scalar = ((first & 15) << 12) | ((second & 63) << 6) | (third & 63);
				return 3;
			}
		}

		if (first >= 240 && first <= 244 && index + 3 < bytes.Length)
		{
			uint32 second = (uint32)bytes[index + 1];
			uint32 third = (uint32)bytes[index + 2];
			uint32 fourth = (uint32)bytes[index + 3];
			bool validSecond = IsContinuation(second) &&
				(first != 240 || second >= 144) &&
				(first != 244 || second <= 143);
			if (validSecond && IsContinuation(third) && IsContinuation(fourth))
			{
				scalar = ((first & 7) << 18) | ((second & 63) << 12) | ((third & 63) << 6) | (fourth & 63);
				return 4;
			}
		}

		scalar = ReplacementCharacter;
		return 1;
	}

	private static bool IsContinuation(uint32 value)
	{
		return value >= 128 && value <= 191;
	}
}
