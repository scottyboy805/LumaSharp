using System.Security.Cryptography;
using System.Text;

namespace LumaSharp.Compiler;

internal sealed record StringLiteralInfo(string Text, string Symbol, byte[] Utf8Bytes, ushort[] Utf16CodeUnits)
{
    public int Length => Utf16CodeUnits.Length;
}

/// <summary>Decodes the string literal forms defined by the LumaSharp language specification.</summary>
internal static class StringLiteralCodec
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static bool TryParse(string raw, out StringLiteralInfo value, out string? error)
    {
        value = null!;
        error = null;
        if (!TryDecode(raw, out var text, out error)) return false;
        return TryCreate(text, out value, out error);
    }

    public static bool TryCreate(string text, out StringLiteralInfo value, out string? error)
    {
        value = null!;
        error = null;
        try
        {
            var utf8 = StrictUtf8.GetBytes(text);
            var utf16 = new ushort[text.Length];
            for (var index = 0; index < text.Length; index++) utf16[index] = text[index];
            var symbol = text.Length == 0 ? "luma_string_empty" : "luma_string_" + Convert.ToHexString(SHA256.HashData(utf8)).ToLowerInvariant();
            value = new(text, symbol, utf8, utf16);
            return true;
        }
        catch (EncoderFallbackException)
        {
            error = "String literal contains an unpaired UTF-16 surrogate and cannot be represented as valid UTF-8.";
            return false;
        }
    }

    private static bool TryDecode(string raw, out string text, out string? error)
    {
        text = "";
        error = null;
        if (raw.StartsWith("@\"", StringComparison.Ordinal))
        {
            if (raw.Length < 3 || raw[^1] != '"') return Fail("Unterminated verbatim string literal.", out error);
            var body = raw.AsSpan(2, raw.Length - 3);
            var result = new StringBuilder(body.Length);
            for (var index = 0; index < body.Length; index++)
            {
                if (body[index] != '"') { result.Append(body[index]); continue; }
                if (index + 1 >= body.Length || body[index + 1] != '"') return Fail("A quote inside a verbatim string must be doubled.", out error);
                result.Append('"'); index++;
            }
            text = result.ToString();
            return true;
        }

        if (raw.Length < 2 || raw[0] != '"' || raw[^1] != '"') return Fail("Unterminated string literal.", out error);
        var content = raw.AsSpan(1, raw.Length - 2);
        var decoded = new StringBuilder(content.Length);
        for (var index = 0; index < content.Length; index++)
        {
            var current = content[index];
            if (current != '\\') { decoded.Append(current); continue; }
            if (++index >= content.Length) return Fail("String literal ends with an incomplete escape sequence.", out error);
            current = content[index];
            switch (current)
            {
                case '0': decoded.Append('\0'); break;
                case 'a': decoded.Append('\a'); break;
                case 'b': decoded.Append('\b'); break;
                case 'f': decoded.Append('\f'); break;
                case 'n': decoded.Append('\n'); break;
                case 'r': decoded.Append('\r'); break;
                case 't': decoded.Append('\t'); break;
                case 'v': decoded.Append('\v'); break;
                case '\\': decoded.Append('\\'); break;
                case '\'': decoded.Append('\''); break;
                case '"': decoded.Append('"'); break;
                case 'x':
                    if (!TryReadHex(content, ref index, 1, 4, out var shortCode)) return Fail("A \\x escape requires one to four hexadecimal digits.", out error);
                    decoded.Append((char)shortCode);
                    break;
                case 'u':
                    if (!TryReadHex(content, ref index, 4, 4, out var utf16Code)) return Fail("A \\u escape requires exactly four hexadecimal digits.", out error);
                    decoded.Append((char)utf16Code);
                    break;
                case 'U':
                    if (!TryReadHex(content, ref index, 8, 8, out var scalar) || !Rune.TryCreate(scalar, out var rune))
                        return Fail("A \\U escape must encode a valid Unicode scalar value.", out error);
                    decoded.Append(rune.ToString());
                    break;
                default: return Fail($"Unknown string escape sequence '\\{current}'.", out error);
            }
        }
        text = decoded.ToString();
        return true;
    }

    private static bool TryReadHex(ReadOnlySpan<char> text, ref int escapeIndex, int minimum, int maximum, out int value)
    {
        value = 0;
        var digits = 0;
        while (escapeIndex + 1 < text.Length && digits < maximum)
        {
            var digit = HexValue(text[escapeIndex + 1]);
            if (digit < 0) break;
            value = (value << 4) | digit;
            escapeIndex++;
            digits++;
        }
        return digits >= minimum;
    }

    private static int HexValue(char value) => value switch
    {
        >= '0' and <= '9' => value - '0',
        >= 'a' and <= 'f' => value - 'a' + 10,
        >= 'A' and <= 'F' => value - 'A' + 10,
        _ => -1
    };

    private static bool Fail(string message, out string? error) { error = message; return false; }
}
