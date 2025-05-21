using LumaSharp.Compiler.AST;
using System.Collections;
using System.Text;

namespace LumaSharp.Compiler.Parser
{
    internal sealed class TokenParser : IEnumerable<SyntaxToken>
    {
        // Private
        private readonly string lineCommentStart = SyntaxToken.GetText(SyntaxTokenKind.LineComment);
        private readonly string blockCommentStart = SyntaxToken.GetText(SyntaxTokenKind.BlockCommentStart);
        private readonly string blockCommentEnd = SyntaxToken.GetText(SyntaxTokenKind.BlockCommentEnd);

        private readonly SyntaxToken[] keywords = SyntaxToken.GetKeywords().Select(t => new SyntaxToken(t)).ToArray();
        private readonly SyntaxToken[] symbols = SyntaxToken.GetSymbols().Select(t => new SyntaxToken(t))
            .OrderByDescending(s => s.Text.Length).ToArray();
    
        private readonly StringBuilder builder = new();
        private readonly TextView source;

        private IEnumerator<SyntaxToken> enumerator;

        // Constructor
        public TokenParser(TextView textView)
        {
            // Check for null
            if(textView == null)
                throw new ArgumentNullException(nameof(textView));

            this.source = textView;
            this.builder = new();
        }

        // Methods
        public IEnumerator<SyntaxToken> GetEnumerator()
        {
            // Check for enumerated
            if (enumerator != null)
                throw new InvalidOperationException("Tokens can only be enumerated once! You should cache the result if you need to iterate further");

            // Need to build the tokens manually
            return enumerator = EnumerateTokens();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private IEnumerator<SyntaxToken> EnumerateTokens()
        {
            SyntaxToken previous = SyntaxToken.Invalid;

            // Create trivia list
            List<SyntaxTrivia> trivia = new();

            // Read all tokens with minimal lookahead
            while(source.EOF == false)
            {
                // Get leading trivia
                ReadLeadingTrivia(source.Position == 0, trivia);

                // Get leading trivia string immediately before the token - used for toking parser separation
                string leadingTriviaString = trivia.Count > 0
                    ? trivia[trivia.Count - 1].Text
                    : string.Empty;


                // ### COMMENT - Check for line or block comment
                if (MatchCommentStart(out SyntaxToken commentStart) == true)
                {
                    // Get the comment start
                    yield return trivia.Count > 0
                        ? commentStart.WithTrivia(trivia)
                        : commentStart;

                    // Get the comment text
                    SyntaxToken commentText = MatchCommentText(commentStart.Kind == SyntaxTokenKind.LineComment
                            ? Environment.NewLine
                            : blockCommentEnd);

                    // Read end trivia
                    if (commentStart.Kind == SyntaxTokenKind.LineComment)
                    {
                        // Read end trivia
                        ReadTrailingTrivia(trivia);
                    }

                    yield return trivia.Count > 0
                        ? commentStart.Kind == SyntaxTokenKind.LineComment
                            ? commentText.WithTrivia(trivia.Where(t => t.IsTrailing))
                            : commentText
                        : commentText;

                    // Get the comment end
                    if (commentStart.Kind == SyntaxTokenKind.BlockCommentStart)
                        yield return previous = MatchCommentEnd(blockCommentEnd);
                }
                // ### LITERAL - Check for literal string
                else if (MatchLiteral(out SyntaxToken literal) == true)
                {
                    // Read end trivia
                    ReadTrailingTrivia(trivia);

                    // Get the literal
                    yield return previous = trivia.Count > 0
                        ? literal.WithTrivia(trivia)
                        : literal;
                }
                // ### KEYWORD - Check for keyword delimited by whitespace or symbol
                else if (MatchKeyword(leadingTriviaString, previous, out SyntaxToken keyword) == true)
                {
                    // Read end trivia
                    ReadTrailingTrivia(trivia);

                    // Get the keyword
                    yield return previous = trivia.Count > 0
                        ? keyword.WithTrivia(trivia)
                        : keyword;
                }
                // ### SYMBOL - Check for symbol delimited by whitespace, letter or digit
                else if (MatchSymbol(leadingTriviaString, previous, out SyntaxToken symbol) == true)
                {
                    // Read end trivia
                    ReadTrailingTrivia(trivia);

                    // Get the symbol
                    yield return previous = trivia.Count > 0
                        ? symbol.WithTrivia(trivia)
                        : symbol;
                }
                // ### NUMBER - Check for numeric value delimited by whitespace or letter
                else if (MatchNumber(leadingTriviaString, previous, out SyntaxToken number, out SyntaxToken descriptor) == true)
                {
                    // Read end trivia
                    ReadTrailingTrivia(trivia);

                    // Get the symbol
                    yield return previous = trivia.Count > 0
                        ? number.WithTrivia(trivia.Where(t => t.IsLeading))
                        : number;

                    // Check for descriptor
                    if(descriptor.Kind != SyntaxTokenKind.Invalid)
                        yield return previous = descriptor;
                }
                // ### IDENTIFIER - Check for identifier name
                else if (MatchIdentifier(leadingTriviaString, previous, out SyntaxToken identifier) == true)
                {
                    // Read end trivia
                    ReadTrailingTrivia(trivia);

                    // Get the symbol
                    yield return previous = trivia.Count > 0
                        ? identifier.WithTrivia(trivia)
                        : identifier;
                }
                
                // ### UNKNOWN - could not parse the string
                else
                {
                    // Try to recover from parser error
                    throw new Exception("Parser error: " + source.Peek());
                }

                // Recycle trivia list
                trivia.Clear();
            }
        }

        #region Trivia
        private void ReadLeadingTrivia(bool isStart, IList<SyntaxTrivia> trivia)
        {
            while(source.EOF == false)
            {
                // Get current char
                char current = source.Peek();

                // Check for leading whitespace
                if(IsWhiteSpaceTrivia(current) == true)
                {
                    builder.Append(current);
                    source.Consume();

                    // Check for build the string
                    if(IsWhiteSpaceTrivia(source.Peek()) == false)
                    {
                        // We can build a whitespace trivia object
                        trivia.Add(SyntaxTrivia.Leading(SyntaxTriviaKind.Whitespace, builder.ToString(), default));
                        builder.Clear();
                    }
                }
                // Check for any other whitespace for starting token only
                else if(isStart == true && IsEndLineTrivia(current) == true)
                {
                    builder.Append(current);
                    source.Consume();

                    // Check for build the string
                    if(IsEndLineTrivia(source.Peek()) == false)
                    {
                        // We can build an end line trivia object
                        trivia.Add(SyntaxTrivia.Leading(SyntaxTriviaKind.Newline, builder.ToString(), default));
                        builder.Clear();
                    }
                }
                else
                {
                    // Must be some other token to be parsed
                    break;
                }
            }
        }

        private void ReadTrailingTrivia(IList<SyntaxTrivia> trivia)
        {
            // First lookahead to see if there is an end line - if not then we should not parse the trailing trivia
            int position = 0;
            while (source.EOF == false)
            {
                // Peek current
                char current = source.Peek(position++);

                // Check for end line
                if (IsEndLineTrivia(current) == true || current == '\0')
                    break;

                // Handle whitespace
                if (IsWhiteSpaceTrivia(current) == true)
                    continue;

                // Must be some other character that should be parsed as a token
                return;
            }

            while (source.EOF == false)
            {
                // Get current char
                char current = source.Peek();

                // Check for whitespace
                if(IsWhiteSpaceTrivia(current) == true)
                {
                    builder.Append(current);
                    source.Consume();

                    // Check for build string
                    if(IsWhiteSpaceTrivia(source.Peek()) == false)
                    {
                        trivia.Add(SyntaxTrivia.Trailing(SyntaxTriviaKind.Whitespace, builder.ToString(), default));
                        builder.Clear();
                    }
                }
                // Check for end line
                else if(IsEndLineTrivia(current) == true)
                {
                    builder.Append(current);
                    source.Consume();

                    // Check for build string
                    if (IsEndLineTrivia(source.Peek()) == false)
                    {
                        trivia.Add(SyntaxTrivia.Trailing(SyntaxTriviaKind.Newline, builder.ToString(), default));
                        builder.Clear();
                    }
                }
                else
                {
                    // Must be end line or some other token to be parsed
                    break;
                }
            }
        }

        private bool IsWhiteSpaceTrivia(char c)
        {
            return char.IsWhiteSpace(c) == true || c == '\t';
        }

        private bool IsEndLineTrivia(char c)
        {
            return c == '\r' || c == '\n';
        }
        #endregion

        private bool MatchCommentStart(out SyntaxToken commentSymbol)
        {            
            // Check for line comment
            if (MatchString(source, lineCommentStart) == true)
            {
                // Consume those characters
                source.Consume(lineCommentStart.Length);

                // Create the token
                commentSymbol = new SyntaxToken(SyntaxTokenKind.LineComment);
                return true;
            }

            // Check for block comment
            if (MatchString(source, blockCommentStart) == true)
            {
                // Consume those characters
                source.Consume(blockCommentStart.Length);

                // Create the token
                commentSymbol = new SyntaxToken(SyntaxTokenKind.BlockCommentStart);
                return true;
            }

            commentSymbol = default;
            return false;
        }

        private SyntaxToken MatchCommentText(string commentEndString)
        {
            // Read until end of line or stream
            while (source.EOF == false && MatchString(source, commentEndString) == false)
            {
                // Get the next comment character
                builder.Append(source.Consume());
            }

            // Create the symbol
            SyntaxToken commentSymbol = new SyntaxToken(SyntaxTokenKind.CommentText, builder.ToString());

            // Recycle builder
            builder.Clear();
            return commentSymbol;
        }

        private SyntaxToken MatchCommentEnd(string commentEndString)
        {
            if (MatchString(source, commentEndString) == false)
                throw new Exception("Invalid comment state");

            source.Consume(commentEndString.Length);
            return new SyntaxToken(SyntaxTokenKind.BlockCommentEnd);
        }

        private bool MatchLiteral(out SyntaxToken literal)
        {
            // Check for quote
            if (source.Peek() == '"')
            {
                // Create trivia for starting quote?
                source.Consume();

                // Read until end of stream or quote
                while(source.EOF == false && source.Peek() != '"')
                {
                    // Append the literal
                    builder.Append(source.Consume());
                }

                // Create trivial for ending quote
                if (source.Peek() != '"')
                {
                    // Expected end quote
                }

                source.Consume();

                // Build the literal
                literal = new SyntaxToken(SyntaxTokenKind.Literal, builder.ToString());

                // Recycle builder
                builder.Clear();
                return true;
            }

            literal = default;
            return false;
        }

        private bool MatchKeyword(string leadingTrivia, in SyntaxToken previousToken, out SyntaxToken keyword)
        {
            // Check for start of doc, white space, end of block comment, or delimiting symbol to start the keyword
            if (source.Position > 0 
                && string.IsNullOrEmpty(leadingTrivia) == true 
                && previousToken.Kind != SyntaxTokenKind.BlockCommentEnd 
                && (previousToken.Kind == SyntaxTokenKind.Invalid || IsKeywordDelimiterCharacter(previousToken.Text.Last()) == false))
            {
                keyword = default;
                return false;
            }

            // Try to match any keyword
            foreach(SyntaxToken keywordToken in keywords)
            {
                // Try to match
                if(MatchString(source, keywordToken.Text) == true)
                {
                    // Get the consume length
                    int length = keywordToken.Text.Length;

                    // Check for end delimiting symbols
                    char end = source.Peek(length);

                    // Check for end of stream, white space, comment start or delimiting symbol
                    // At the end of a keyword we must have either whitespace, end of stream or a valid delimiting character such as a symbol or number
                    if(end != '\0' 
                        && IsWhiteSpaceTrivia(end) == false 
                        && MatchString(source, lineCommentStart, length) == false 
                        && MatchString(source, blockCommentStart, length) == false 
                        && IsKeywordDelimiterCharacter(end) == false)
                    {
                        // Make be an identifier that starts with a keyword
                        continue;
                    }

                    // Now we are certain that it is a keyword with suitable delimiters, so consume the keyword
                    source.Consume(length);

                    // Update token
                    keyword = keywordToken;
                    return true;
                }
            }

            // Not a keyword
            keyword = default;
            return false;
        }

        private bool MatchSymbol(string leadingTrivia, in SyntaxToken previousToken, out SyntaxToken symbol)
        {
            // Check for start of doc, white space, end of block comment, or delimiting symbol to start the keyword
            if (source.Position > 0
                && string.IsNullOrEmpty(leadingTrivia) == true
                && previousToken.Kind != SyntaxTokenKind.BlockCommentEnd
                && (previousToken.Kind == SyntaxTokenKind.Invalid || IsSymbolDelimiterCharacter(previousToken.Text.Last()) == false))
            {
                symbol = default;
                return false;
            }

            // Try to match symbol
            foreach (SyntaxToken symbolToken in symbols)
            {
                // Try to match
                if (MatchString(source, symbolToken.Text) == true)
                {
                    // Get the consume length
                    int length = symbolToken.Text.Length;

                    // Check for end delimiting symbols
                    char end = source.Peek(length);

                    // Check for end of stream, white space, comment start or delimiting symbol
                    // At the end of a symbol we must have either whitespace, end of stream or a valid delimiting character such as a symbol or number
                    if (end != '\0'
                        && IsWhiteSpaceTrivia(end) == false
                        && MatchString(source, lineCommentStart, length) == false
                        && MatchString(source, blockCommentStart, length) == false
                        && IsSymbolDelimiterCharacter(end) == false)
                    {
                        // Make be an identifier that starts with a keyword
                        continue;
                    }

                    // Now we are certain that it is a symbol with suitable delimiters, so consume the symbol
                    source.Consume(length);

                    // Update token
                    symbol = symbolToken;
                    return true;
                }
            }

            // Not a symbol
            symbol = default;
            return false;
        }

        private bool MatchNumber(string leadingTrivia, in SyntaxToken previousToken, out SyntaxToken number, out SyntaxToken descriptor)
        {
            descriptor = default;

            // Require whitespace or symbol before number
            if (source.Position > 0
                && string.IsNullOrEmpty(leadingTrivia) == true
                && previousToken.Kind != SyntaxTokenKind.BlockCommentEnd
                && (previousToken.Kind == SyntaxTokenKind.Invalid || IsKeywordDelimiterCharacter(previousToken.Text.Last()) == false))
            {
                number = default;                
                return false;
            }

            // Get first char
            char first = source.Peek();

            // Check for first number or decimal ('.5' is acceptable)
            if (char.IsNumber(first) == true || first == '.')
            {
                // Consume first value
                builder.Append(source.Consume());

                // Check for decimal point consumed
                bool consumedDecimal = first == '.';

                // Consume all remaining digits
                while(char.IsDigit(source.Peek()) == true || (consumedDecimal == false && source.Peek() == '.'))
                {
                    // Set flag
                    if (source.Peek() == '.')
                        consumedDecimal = true;

                    // Append the character
                    builder.Append(source.Consume());
                }

                // Build the full number string
                number = new SyntaxToken(SyntaxTokenKind.Literal, builder.ToString());

                // Check for descriptor
                // Unsigned
                if(source.Peek() == 'U' || source.Peek() == 'u')
                {
                    // Check for following L
                    if(source.Peek(1) == 'L' || source.Peek(1) == 'l')
                    {
                        source.Consume(2);
                        descriptor = new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "UL");
                    }
                    else
                    {
                        source.Consume();
                        descriptor = new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "U");
                    }
                }
                // Long
                else if(source.Peek() == 'L' || source.Peek() == 'l')
                {
                    source.Consume();
                    descriptor = new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "L");
                }
                // Float
                else if(source.Peek() == 'F' || source.Peek() == 'f')
                {
                    source.Consume();
                    descriptor = new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "F");
                }
                // Double
                else if(source.Peek() == 'D' || source.Peek() == 'd')
                {
                    source.Consume();
                    descriptor = new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "D");
                }

                // Recycle builder
                builder.Clear();
                return true;
            }

            // Not a number
            number = default;
            return false;
        }

        private bool MatchIdentifier(string leadingTrivia, in SyntaxToken previousToken, out SyntaxToken identifier)
        {
            // Require whitespace or symbol before identifier
            if (source.Position > 0
                && string.IsNullOrEmpty(leadingTrivia) == true
                && previousToken.Kind != SyntaxTokenKind.BlockCommentEnd
                && (previousToken.Kind == SyntaxTokenKind.Invalid || IsKeywordDelimiterCharacter(previousToken.Text.Last()) == false))
            {
                identifier = default;
                return false;
            }

            // Get first character
            char first = source.Peek();

            // Require letter or underscore to start an identifier
            if(char.IsLetter(first) == true || first == '_')
            {
                // Read thr first character
                builder.Append(source.Consume());

                // Read all supported characters
                while(source.EOF == false
                    && (char.IsLetterOrDigit(source.Peek()) == true || source.Peek() == '_'))
                {
                    // Append the character
                    builder.Append(source.Consume());
                }

                // Create the token
                identifier = new SyntaxToken(SyntaxTokenKind.Identifier, builder.ToString());

                // Recycle builder
                builder.Clear();
                return true;
            }

            // Not an identifier
            identifier = default;
            return false;
        }

        private static bool MatchAnyString(TextView view, string[] matches, out string match)
        {
            foreach(string possibleMatch in matches)
            {
                // Check for match
                if(MatchString(view, possibleMatch) == true)
                {
                    match = possibleMatch;
                    return true;
                }
            }

            match = null;
            return false;
        }

        private static bool MatchString(TextView view, string match, int offset = 0)
        {
            // Check bounds
            if(offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            for(int i = 0; i < match.Length; i++)
            {
                // Check for match or exit early
                if (view.Peek(i + offset) != match[i])
                    return false;
            }

            // Must be a match
            return true;
        }

        private static bool IsKeywordDelimiterCharacter(char c)
        {
            // Check for text or quote or underscore
            return char.IsLetter(c) == false && c != '"' && c != '\'' && c != '_';
        }

        private static bool IsSymbolDelimiterCharacter(char c)
        {
            // Check for text or digit or quote
            return char.IsLetterOrDigit(c) == true || c == '"' || c == '\'' || c != '_';
        }
    }
}
