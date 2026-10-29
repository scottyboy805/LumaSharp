namespace LumaSharp.Compiler;

internal abstract record TypeNameSuffix;
internal sealed record NullableTypeNameSuffix : TypeNameSuffix
{
    public override string ToString() => "?";
}
internal sealed record PointerTypeNameSuffix : TypeNameSuffix
{
    public override string ToString() => "*";
}
internal sealed record ArrayRankTypeNameSuffix(int Rank) : TypeNameSuffix
{
    public override string ToString() => "[" + new string(',', Rank - 1) + "]";
}
internal sealed record InlineArrayTypeNameSuffix(string LengthExpression) : TypeNameSuffix
{
    public override string ToString() => "[" + LengthExpression + "]";
}

/// <summary>
/// A parsed type name used by binding and generic specialization. The syntax tree
/// keeps generic arguments and postfix type constructors separate from the base
/// name so substitutions cannot accidentally match substrings inside identifiers.
/// </summary>
internal sealed record TypeNameSyntax(
    string Name,
    IReadOnlyList<TypeNameSyntax> Arguments,
    IReadOnlyList<TypeNameSuffix> Suffixes)
{
    public static bool TryParse(string text, out TypeNameSyntax syntax)
    {
        var parser = new Parser(text);
        if (parser.TryParseType(out syntax) && parser.AtEnd)
            return true;
        syntax = null!;
        return false;
    }

    public IEnumerable<TypeNameSyntax> DescendantsAndSelf()
    {
        yield return this;
        foreach (var argument in Arguments)
        foreach (var child in argument.DescendantsAndSelf())
            yield return child;
    }

    public TypeNameSyntax Rewrite(IReadOnlyDictionary<string, string> substitutions) =>
        Rewrite(substitutions, new HashSet<string>(StringComparer.Ordinal));

    private TypeNameSyntax Rewrite(IReadOnlyDictionary<string, string> substitutions, HashSet<string> resolving)
    {
        var closedName = Arguments.Count == 0 ? Name : Name + "<" + string.Join(",", Arguments) + ">";
        if (substitutions.TryGetValue(closedName, out var closedReplacement) && resolving.Add(closedName))
        {
            try
            {
                if (TryParse(closedReplacement, out var parsedReplacement))
                {
                    var rewrittenReplacement = parsedReplacement.Rewrite(substitutions, resolving);
                    return rewrittenReplacement with
                    {
                        Suffixes = [.. rewrittenReplacement.Suffixes, .. Suffixes]
                    };
                }
            }
            finally
            {
                resolving.Remove(closedName);
            }
        }

        if (Arguments.Count == 0 && substitutions.TryGetValue(Name, out var replacement) && resolving.Add(Name))
        {
            try
            {
                if (TryParse(replacement, out var parsedReplacement))
                {
                    var rewrittenReplacement = parsedReplacement.Rewrite(substitutions, resolving);
                    return rewrittenReplacement with
                    {
                        Suffixes = [.. rewrittenReplacement.Suffixes, .. Suffixes]
                    };
                }
            }
            finally
            {
                resolving.Remove(Name);
            }
        }

        return this with
        {
            Arguments = Arguments.Select(argument => argument.Rewrite(substitutions, resolving)).ToArray()
        };
    }

    public override string ToString() =>
        Name +
        (Arguments.Count == 0 ? string.Empty : "<" + string.Join(",", Arguments) + ">") +
        string.Concat(Suffixes);

    private sealed class Parser(string text)
    {
        private int _position;

        public bool AtEnd
        {
            get
            {
                SkipWhitespace();
                return _position == text.Length;
            }
        }

        public bool TryParseType(out TypeNameSyntax syntax)
        {
            SkipWhitespace();
            var nameStart = _position;
            while (_position < text.Length && IsNameCharacter(text[_position])) _position++;
            if (_position == nameStart)
            {
                syntax = null!;
                return false;
            }

            var name = text[nameStart.._position];
            var arguments = new List<TypeNameSyntax>();
            SkipWhitespace();
            if (Eat('<'))
            {
                SkipWhitespace();
                if (Eat('>'))
                {
                    syntax = null!;
                    return false;
                }

                while (true)
                {
                    if (!TryParseType(out var argument))
                    {
                        syntax = null!;
                        return false;
                    }
                    arguments.Add(argument);
                    SkipWhitespace();
                    if (Eat('>')) break;
                    if (!Eat(','))
                    {
                        syntax = null!;
                        return false;
                    }
                }
            }

            var suffixes = new List<TypeNameSuffix>();
            while (true)
            {
                SkipWhitespace();
                if (Eat('?')) suffixes.Add(new NullableTypeNameSuffix());
                else if (Eat('*')) suffixes.Add(new PointerTypeNameSuffix());
                else if (Eat('['))
                {
                    var contentStart = _position;
                    var depth = 1;
                    var closed = false;
                    while (_position < text.Length)
                    {
                        var current = text[_position++];
                        if (current == '[') depth++;
                        else if (current == ']')
                        {
                            if (--depth == 0)
                            {
                                closed = true;
                                break;
                            }
                        }
                    }
                    if (!closed)
                    {
                        syntax = null!;
                        return false;
                    }

                    var content = text[contentStart..(_position - 1)].Trim();
                    if (content.All(character => character == ',' || char.IsWhiteSpace(character)))
                        suffixes.Add(new ArrayRankTypeNameSuffix(content.Count(character => character == ',') + 1));
                    else
                        suffixes.Add(new InlineArrayTypeNameSuffix(content));
                }
                else break;
            }

            syntax = new(name, arguments, suffixes);
            return true;
        }

        private void SkipWhitespace()
        {
            while (_position < text.Length && char.IsWhiteSpace(text[_position])) _position++;
        }

        private bool Eat(char value)
        {
            if (_position >= text.Length || text[_position] != value) return false;
            _position++;
            return true;
        }

        private static bool IsNameCharacter(char value) =>
            char.IsLetterOrDigit(value) || value is '_' or '$' or '.' or '@';
    }
}
