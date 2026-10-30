using System.Globalization;
using System.Text;

namespace LumaSharp.Compiler;

internal enum TokenKind { Identifier, Number, String, Character, Punctuation, End }
internal sealed record Token(TokenKind Kind, string Text, SourceLocation Location);

internal sealed class Lexer(string file, string source, DiagnosticBag diagnostics, CancellationToken cancellationToken = default)
{
    private static readonly string[] Operators = [">>=", "<<=", "??=", "...", "?.", "?[", "??", "=>", "==", "!=", "<=", ">=", "&&", "||", "++", "--", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<", ">>"];
    private int _index, _line = 1, _column = 1;
    private int _cancellationCheckCounter;
    public IReadOnlyList<Token> Scan()
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tokens = new List<Token>();
        while (!AtEnd)
        {
            if (char.IsWhiteSpace(Current)) { Advance(); continue; }
            if (Current == '/' && Peek(1) == '/') { while (!AtEnd && Current is not ('\r' or '\n')) Advance(); continue; }
            if (Current == '/' && Peek(1) == '*') { ScanBlockComment(); continue; }
            var location = Location;
            if (Current == '@' && Peek(1) == '"') { tokens.Add(ScanVerbatimString()); continue; }
            if (Current == '"') { tokens.Add(ScanQuoted('"', TokenKind.String)); continue; }
            if (Current == '\'') { tokens.Add(ScanQuoted('\'', TokenKind.Character)); continue; }
            if (IsIdentifierStartAt(_index)) { tokens.Add(ScanIdentifier()); continue; }
            if (char.IsDigit(Current)) { tokens.Add(ScanNumber()); continue; }
            var op = Operators.FirstOrDefault(o => source.AsSpan(_index).StartsWith(o, StringComparison.Ordinal));
            if (op is not null) { foreach (var _ in op) Advance(); tokens.Add(new(TokenKind.Punctuation, op, location)); continue; }
            if ("{}[](),.:;?+-*/%&|^~!=<>".Contains(Current)) { tokens.Add(new(TokenKind.Punctuation, Advance().ToString(), location)); continue; }
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"Unexpected character U+{(int)Current:X4}.", location); Advance();
        }
        tokens.Add(new(TokenKind.End, "<eof>", Location));
        return tokens;
    }
    private bool AtEnd => _index >= source.Length;
    private char Current => AtEnd ? '\0' : source[_index];
    private char Peek(int offset) => _index + offset < source.Length ? source[_index + offset] : '\0';
    private SourceLocation Location => new(file, _line, _column);
    private char Advance()
    {
        if ((++_cancellationCheckCounter & 0x3ff) == 0) cancellationToken.ThrowIfCancellationRequested();
        var c = source[_index++];
        if (c == '\r') { if (_index < source.Length && source[_index] == '\n') _index++; _line++; _column = 1; return '\n'; }
        if (c == '\n') { _line++; _column = 1; } else _column++;
        return c;
    }
    private bool IsIdentifierStartAt(int index)
    {
        if (index >= source.Length) return false;
        if (source[index] == '@') return true;
        return Rune.DecodeFromUtf16(source.AsSpan(index), out var rune, out _) == System.Buffers.OperationStatus.Done && IsIdentifierStartRune(rune);
    }
    private static bool IsIdentifierPartRune(Rune rune) => IsIdentifierStartRune(rune) || Rune.GetUnicodeCategory(rune) == UnicodeCategory.DecimalDigitNumber;
    private static bool IsIdentifierStartRune(Rune rune) => rune.Value == '_' || Rune.GetUnicodeCategory(rune) is UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter;
    private Token ScanIdentifier()
    {
        var location = Location; var start = _index; var escaped = Current == '@'; if (escaped) Advance();
        var identifierStart = _index;
        while (!AtEnd)
        {
            var status = Rune.DecodeFromUtf16(source.AsSpan(_index), out var rune, out var consumed);
            if (status != System.Buffers.OperationStatus.Done || !(_index == identifierStart ? IsIdentifierStartRune(rune) : IsIdentifierPartRune(rune))) break;
            for (var i = 0; i < consumed; i++) Advance();
        }
        var text = source[start.._index];
        return new(TokenKind.Identifier, text, location);
    }
    private Token ScanNumber()
    {
        var location = Location; var start = _index;
        if (Current == '0' && Peek(1) is 'x' or 'X' or 'b' or 'B')
        {
            Advance(); Advance();
            while (char.IsLetterOrDigit(Current) || Current == '_') Advance();
        }
        else
        {
            while (char.IsAsciiDigit(Current) || Current == '_') Advance();
            if (Current == '.' && Peek(1) != '.')
            {
                Advance();
                while (char.IsAsciiDigit(Current) || Current == '_') Advance();
            }
            if (Current is 'e' or 'E')
            {
                Advance();
                if (Current is '+' or '-') Advance();
                while (char.IsAsciiDigit(Current) || Current == '_') Advance();
            }
            // Keep invalid alphanumeric suffixes in this token so they diagnose as one malformed literal.
            while (char.IsLetterOrDigit(Current) || Current == '_') Advance();
        }
        return new(TokenKind.Number, source[start.._index], location);
    }
    private Token ScanQuoted(char quote, TokenKind kind)
    {
        var location = Location; var start = _index; Advance(); var closed = false;
        while (!AtEnd)
        {
            if (Current == quote) { Advance(); closed = true; break; }
            if (Current is '\r' or '\n') break;
            if (Current == '\\') { Advance(); if (!AtEnd) Advance(); } else Advance();
        }
        if (!closed) diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Unterminated literal.", location);
        return new(kind, source[start.._index], location);
    }
    private Token ScanVerbatimString()
    {
        var location = Location; var start = _index; Advance(); Advance();
        while (!AtEnd) { if (Current == '"') { Advance(); if (Current == '"') { Advance(); continue; } return new(TokenKind.String, source[start.._index], location); } Advance(); }
        diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Unterminated verbatim string.", location); return new(TokenKind.String, source[start.._index], location);
    }
    private void ScanBlockComment()
    {
        var at = Location; Advance(); Advance();
        while (!AtEnd && !(Current == '*' && Peek(1) == '/')) Advance();
        if (AtEnd) diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Unterminated block comment.", at); else { Advance(); Advance(); }
    }
}
