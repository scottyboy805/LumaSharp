using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LumaSharp.Compiler;

internal sealed record DisabledDiagnosticSpan(string Code, int StartLine, int EndLine);

internal sealed record PreprocessorResult(
    string File,
    string Source,
    IReadOnlyList<Diagnostic> Diagnostics,
    IReadOnlyList<DisabledDiagnosticSpan> DisabledDiagnostics)
{
    public bool Disables(Diagnostic diagnostic)
    {
        if (diagnostic.Location is not { } location || !string.Equals(location.File, File, StringComparison.Ordinal))
            return false;
        foreach (var span in DisabledDiagnostics)
            if (span.StartLine <= location.Line && location.Line <= span.EndLine &&
                string.Equals(span.Code, diagnostic.Code, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }
}

/// <summary>Processes source directives before lexing while preserving every source line.</summary>
internal static class SourcePreprocessor
{
    public const string DirectiveErrorCode = "LS0002";
    public const string ErrorDirectiveCode = "LS0003";
    public const string WarningDirectiveCode = "LS3005";

    private sealed class ConditionalFrame(bool parentActive, bool branchTaken, SourceLocation location)
    {
        public bool ParentActive { get; } = parentActive;
        public bool BranchTaken { get; set; } = branchTaken;
        public bool ElseSeen { get; set; }
        public SourceLocation Location { get; } = location;
    }

    private sealed class LexicalState
    {
        public bool InBlockComment { get; set; }
        public bool InVerbatimString { get; set; }
    }

    public static PreprocessorResult Process(string file, string source, IReadOnlySet<string>? projectSymbols,
        CancellationToken cancellationToken)
    {
        var output = source.ToCharArray();
        if (output.Length > 0 && output[0] == '\uFEFF') output[0] = ' ';
        var diagnostics = new List<Diagnostic>();
        var disabledDiagnostics = new List<DisabledDiagnosticSpan>();
        var disabledStarts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var symbols = new HashSet<string>(StringComparer.Ordinal);
        if (projectSymbols is not null)
        {
            foreach (var symbol in projectSymbols)
            {
                if (IsValidSymbol(symbol)) symbols.Add(symbol);
                else diagnostics.Add(new(DirectiveErrorCode, DiagnosticSeverity.Error,
                    $"Project preprocessor symbol '{symbol}' is not a valid identifier.", new(file, 1, 1)));
            }
        }

        var conditionals = new List<ConditionalFrame>();
        var lexicalState = new LexicalState();
        var active = true;
        var seenActiveCode = false;
        var lineNumber = 1;
        var lineStart = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var lineEnd = lineStart;
            while (lineEnd < source.Length && source[lineEnd] is not ('\r' or '\n')) lineEnd++;
            var line = source.AsSpan(lineStart, lineEnd - lineStart);
            var first = FirstNonWhitespace(line);
            var isDirective = first < line.Length && line[first] == '#' &&
                (!active || (!lexicalState.InBlockComment && !lexicalState.InVerbatimString));

            if (isDirective)
            {
                Blank(output, lineStart, lineEnd);
                ProcessDirective(file, line, first, lineNumber, symbols, conditionals, disabledStarts,
                    disabledDiagnostics, diagnostics, ref active, ref seenActiveCode);
            }
            else if (!active)
            {
                Blank(output, lineStart, lineEnd);
            }
            else if (UpdateLexicalState(line, lexicalState))
            {
                seenActiveCode = true;
            }

            if (lineEnd == source.Length) break;
            lineStart = lineEnd + (source[lineEnd] == '\r' && lineEnd + 1 < source.Length && source[lineEnd + 1] == '\n' ? 2 : 1);
            lineNumber++;
        }

        foreach (var conditional in conditionals)
            diagnostics.Add(new(DirectiveErrorCode, DiagnosticSeverity.Error,
                "Conditional directive block is missing '#endif'.", conditional.Location));
        foreach (var (code, startLine) in disabledStarts)
            disabledDiagnostics.Add(new(code, startLine, lineNumber));

        return new(file, new string(output), diagnostics, disabledDiagnostics);
    }

    public static bool IsValidSymbol(string? symbol)
    {
        if (string.IsNullOrEmpty(symbol) || symbol is "true" or "false") return false;
        var first = true;
        var index = 0;
        while (index < symbol.Length)
        {
            if (Rune.DecodeFromUtf16(symbol.AsSpan(index), out var rune, out var consumed) != System.Buffers.OperationStatus.Done)
                return false;
            var category = Rune.GetUnicodeCategory(rune);
            var valid = rune.Value == '_' || category is UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or
                UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter ||
                !first && category == UnicodeCategory.DecimalDigitNumber;
            if (!valid) return false;
            index += consumed;
            first = false;
        }
        return !first;
    }

    public static bool TryNormalizeDiagnosticCode(string text, out string code)
    {
        var candidate = text.Trim();
        if (Regex.IsMatch(candidate, "^[0-9]{4}$", RegexOptions.CultureInvariant))
        {
            code = "LS" + candidate;
            return true;
        }
        if (Regex.IsMatch(candidate, "^(LS|LMC)[0-9]{4}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            code = candidate.ToUpperInvariant();
            return true;
        }
        code = string.Empty;
        return false;
    }

    private static void ProcessDirective(string file, ReadOnlySpan<char> line, int hashIndex, int lineNumber,
        HashSet<string> symbols, List<ConditionalFrame> conditionals, Dictionary<string, int> disabledStarts,
        List<DisabledDiagnosticSpan> disabledDiagnostics, List<Diagnostic> diagnostics,
        ref bool active, ref bool seenActiveCode)
    {
        var location = new SourceLocation(file, lineNumber, hashIndex + 1);
        var content = StripLineComment(line[(hashIndex + 1)..]).Trim();
        var nameLength = 0;
        while (nameLength < content.Length && IsAsciiLetter(content[nameLength])) nameLength++;
        if (nameLength == 0)
        {
            AddDirectiveError(diagnostics, "Expected a preprocessor directive name after '#'.", location);
            return;
        }

        var name = content[..nameLength].ToString();
        var argument = content[nameLength..].Trim();
        switch (name)
        {
            case "define":
                if (!TrySingleIdentifier(argument, out var symbol))
                    AddDirectiveError(diagnostics, "'#define' requires exactly one valid symbol name.", location);
                else if (active)
                {
                    if (seenActiveCode)
                        AddDirectiveError(diagnostics, "'#define' must appear before source code in its file.", location);
                    else
                        symbols.Add(symbol);
                }
                break;
            case "if":
            {
                var parentActive = active;
                if (!TryEvaluateExpression(argument, symbols, out var condition, out var error))
                {
                    AddDirectiveError(diagnostics, error!, location);
                    condition = false;
                }
                conditionals.Add(new(parentActive, condition, location));
                active = parentActive && condition;
                break;
            }
            case "elif":
            {
                if (conditionals.Count == 0)
                {
                    AddDirectiveError(diagnostics, "'#elif' has no matching '#if'.", location);
                    break;
                }
                var frame = conditionals[^1];
                if (frame.ElseSeen)
                {
                    AddDirectiveError(diagnostics, "'#elif' cannot appear after '#else'.", location);
                    active = false;
                    break;
                }
                if (!TryEvaluateExpression(argument, symbols, out var condition, out var error))
                {
                    AddDirectiveError(diagnostics, error!, location);
                    condition = false;
                }
                active = frame.ParentActive && !frame.BranchTaken && condition;
                frame.BranchTaken |= condition;
                break;
            }
            case "else":
                if (argument.Length != 0)
                    AddDirectiveError(diagnostics, "'#else' does not accept an expression.", location);
                if (conditionals.Count == 0)
                    AddDirectiveError(diagnostics, "'#else' has no matching '#if'.", location);
                else
                {
                    var frame = conditionals[^1];
                    if (frame.ElseSeen)
                    {
                        AddDirectiveError(diagnostics, "A conditional block may contain only one '#else'.", location);
                        active = false;
                    }
                    else
                    {
                        frame.ElseSeen = true;
                        active = frame.ParentActive && !frame.BranchTaken;
                        frame.BranchTaken = true;
                    }
                }
                break;
            case "endif":
                if (argument.Length != 0)
                    AddDirectiveError(diagnostics, "'#endif' does not accept an expression.", location);
                if (conditionals.Count == 0)
                    AddDirectiveError(diagnostics, "'#endif' has no matching '#if'.", location);
                else
                {
                    active = conditionals[^1].ParentActive;
                    conditionals.RemoveAt(conditionals.Count - 1);
                }
                break;
            case "warning":
                if (active)
                    diagnostics.Add(new(WarningDirectiveCode, DiagnosticSeverity.Warning,
                        argument.Length == 0 ? "Preprocessor '#warning' directive." : argument.ToString(), location));
                break;
            case "error":
                if (active)
                    diagnostics.Add(new(ErrorDirectiveCode, DiagnosticSeverity.Error,
                        argument.Length == 0 ? "Preprocessor '#error' directive." : argument.ToString(), location));
                break;
            case "disable":
            case "enable":
                if (argument.Length == 0)
                {
                    AddDirectiveError(diagnostics, $"'#{name}' requires one or more diagnostic codes.", location);
                    break;
                }
                if (!active) break;
                foreach (var part in argument.ToString().Split(','))
                {
                    if (!TryNormalizeDiagnosticCode(part, out var code))
                    {
                        AddDirectiveError(diagnostics, $"'{part.Trim()}' is not a valid diagnostic code.", location);
                        continue;
                    }
                    if (name == "disable")
                        disabledStarts.TryAdd(code, lineNumber + 1);
                    else if (disabledStarts.Remove(code, out var startLine) && lineNumber - 1 >= startLine)
                        disabledDiagnostics.Add(new(code, startLine, lineNumber - 1));
                }
                break;
            default:
                AddDirectiveError(diagnostics, $"Unknown preprocessor directive '#{name}'.", location);
                break;
        }
    }

    private static void AddDirectiveError(List<Diagnostic> diagnostics, string message, SourceLocation location) =>
        diagnostics.Add(new(DirectiveErrorCode, DiagnosticSeverity.Error, message, location));

    private static int FirstNonWhitespace(ReadOnlySpan<char> line)
    {
        var index = 0;
        while (index < line.Length && (char.IsWhiteSpace(line[index]) || index == 0 && line[index] == '\uFEFF')) index++;
        return index;
    }

    private static void Blank(char[] output, int start, int end)
    {
        for (var index = start; index < end; index++) output[index] = ' ';
    }

    private static ReadOnlySpan<char> StripLineComment(ReadOnlySpan<char> text)
    {
        var comment = text.IndexOf("//", StringComparison.Ordinal);
        return comment < 0 ? text : text[..comment];
    }

    private static bool TrySingleIdentifier(ReadOnlySpan<char> text, out string symbol)
    {
        var trimmed = text.Trim();
        symbol = trimmed.ToString();
        if (!IsValidSymbol(symbol)) return false;
        return true;
    }

    private static bool IsAsciiLetter(char value) => value is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    private static bool UpdateLexicalState(ReadOnlySpan<char> line, LexicalState state)
    {
        var hasCode = false;
        var index = 0;
        while (index < line.Length)
        {
            if (state.InBlockComment)
            {
                var close = line[index..].IndexOf("*/", StringComparison.Ordinal);
                if (close < 0) return hasCode;
                index += close + 2;
                state.InBlockComment = false;
                continue;
            }
            if (state.InVerbatimString)
            {
                if (line[index] == '"')
                {
                    if (index + 1 < line.Length && line[index + 1] == '"') { index += 2; continue; }
                    state.InVerbatimString = false;
                }
                index++;
                continue;
            }

            if (char.IsWhiteSpace(line[index])) { index++; continue; }
            if (index + 1 < line.Length && line[index] == '/' && line[index + 1] == '/') break;
            if (index + 1 < line.Length && line[index] == '/' && line[index + 1] == '*')
            {
                state.InBlockComment = true;
                index += 2;
                continue;
            }

            var quoteIndex = GetStringQuoteIndex(line, index, out var isVerbatim, out var isCharacter);
            if (quoteIndex >= 0)
            {
                hasCode = true;
                index = quoteIndex + 1;
                if (isVerbatim)
                {
                    state.InVerbatimString = true;
                    continue;
                }
                var quote = isCharacter ? '\'' : '"';
                while (index < line.Length)
                {
                    if (line[index] == '\\') { index += Math.Min(2, line.Length - index); continue; }
                    if (line[index++] == quote) break;
                }
                continue;
            }

            hasCode = true;
            index++;
        }
        return hasCode;
    }

    private static int GetStringQuoteIndex(ReadOnlySpan<char> line, int index, out bool isVerbatim, out bool isCharacter)
    {
        isVerbatim = false;
        isCharacter = false;
        if (line[index] is '"' or '\'')
        {
            isCharacter = line[index] == '\'';
            return index;
        }
        var cursor = index;
        if (line[cursor] == '$') cursor++;
        if (cursor < line.Length && line[cursor] == '@') { isVerbatim = true; cursor++; }
        else if (cursor == index && line[cursor] == '@') { isVerbatim = true; cursor++; if (cursor < line.Length && line[cursor] == '$') cursor++; }
        if (cursor < line.Length && line[cursor] == '"') return cursor;
        isVerbatim = false;
        return -1;
    }

    private static bool TryEvaluateExpression(ReadOnlySpan<char> expression, HashSet<string> symbols,
        out bool value, out string? error)
    {
        var parser = new ConditionalExpressionParser(expression, symbols);
        return parser.TryParse(out value, out error);
    }

    private sealed class ConditionalExpressionParser
    {
        private readonly string _source;
        private readonly HashSet<string> _symbols;
        private readonly List<string> _tokens = [];
        private int _index;
        private string? _error;

        public ConditionalExpressionParser(ReadOnlySpan<char> source, HashSet<string> symbols)
        {
            _source = source.ToString();
            _symbols = symbols;
        }

        public bool TryParse(out bool value, out string? error)
        {
            value = false;
            error = null;
            if (!Tokenize()) { error = _error; return false; }
            if (_tokens.Count == 0) { error = "'#if' requires a conditional expression."; return false; }
            if (!ParseOr(out value))
            {
                error = _error ?? "Invalid conditional expression.";
                return false;
            }
            if (_index != _tokens.Count)
            {
                error = $"Unexpected token '{_tokens[_index]}' in conditional expression.";
                return false;
            }
            return true;
        }

        private bool Tokenize()
        {
            var index = 0;
            while (index < _source.Length)
            {
                if (char.IsWhiteSpace(_source[index])) { index++; continue; }
                if (index + 1 < _source.Length && _source.AsSpan(index).StartsWith("&&", StringComparison.Ordinal))
                { _tokens.Add("&&"); index += 2; continue; }
                if (index + 1 < _source.Length && _source.AsSpan(index).StartsWith("||", StringComparison.Ordinal))
                { _tokens.Add("||"); index += 2; continue; }
                if (index + 1 < _source.Length && _source.AsSpan(index).StartsWith("==", StringComparison.Ordinal))
                { _tokens.Add("=="); index += 2; continue; }
                if (index + 1 < _source.Length && _source.AsSpan(index).StartsWith("!=", StringComparison.Ordinal))
                { _tokens.Add("!="); index += 2; continue; }
                if (_source[index] is '!' or '(' or ')') { _tokens.Add(_source[index++].ToString()); continue; }
                var runeStatus = Rune.DecodeFromUtf16(_source.AsSpan(index), out var rune, out var consumed);
                if (runeStatus == System.Buffers.OperationStatus.Done && IsIdentifierStart(rune))
                {
                    var start = index;
                    index += consumed;
                    while (index < _source.Length && Rune.DecodeFromUtf16(_source.AsSpan(index), out rune, out consumed) == System.Buffers.OperationStatus.Done && IsIdentifierPart(rune))
                        index += consumed;
                    _tokens.Add(_source[start..index].ToString());
                    continue;
                }
                _error = $"Invalid character '{_source[index]}' in conditional expression.";
                return false;
            }
            return true;
        }

        private bool ParseOr(out bool value)
        {
            if (!ParseAnd(out value)) return false;
            while (Take("||"))
            {
                if (!ParseAnd(out var right)) return false;
                value |= right;
            }
            return true;
        }

        private bool ParseAnd(out bool value)
        {
            if (!ParseEquality(out value)) return false;
            while (Take("&&"))
            {
                if (!ParseEquality(out var right)) return false;
                value &= right;
            }
            return true;
        }

        private bool ParseEquality(out bool value)
        {
            if (!ParseUnary(out value)) return false;
            while (_index < _tokens.Count && (_tokens[_index] is "==" or "!="))
            {
                var operation = _tokens[_index++];
                if (!ParseUnary(out var right)) return false;
                value = operation == "==" ? value == right : value != right;
            }
            return true;
        }

        private bool ParseUnary(out bool value)
        {
            if (Take("!"))
            {
                if (!ParseUnary(out value)) return false;
                value = !value;
                return true;
            }
            if (Take("("))
            {
                if (!ParseOr(out value) || !Take(")"))
                {
                    _error ??= "Expected ')' in conditional expression.";
                    return false;
                }
                return true;
            }
            if (_index >= _tokens.Count)
            {
                _error = "Expected a symbol or parenthesized expression in conditional expression.";
                value = false;
                return false;
            }
            var symbol = _tokens[_index++];
            if (symbol is "true" or "false") { value = symbol == "true"; return true; }
            if (!IsValidSymbol(symbol))
            {
                _error = $"Expected a symbol in conditional expression, found '{symbol}'.";
                value = false;
                return false;
            }
            value = _symbols.Contains(symbol);
            return true;
        }

        private bool Take(string token)
        {
            if (_index >= _tokens.Count || _tokens[_index] != token) return false;
            _index++;
            return true;
        }
    }

    private static bool IsIdentifierStart(Rune rune) => rune.Value == '_' || Rune.GetUnicodeCategory(rune) is
        UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or UnicodeCategory.TitlecaseLetter or
        UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter;

    private static bool IsIdentifierPart(Rune rune) => IsIdentifierStart(rune) || Rune.GetUnicodeCategory(rune) == UnicodeCategory.DecimalDigitNumber;
}
