using LumaSharp.Compiler.AST;
using System.Collections;
using System.Text;

namespace LumaSharp.Compiler.Parser
{
    internal sealed class TokenParser : IEnumerable<SyntaxToken>
    {
        // Private
        private readonly SyntaxToken[] keywords = SyntaxToken.GetKeywords().Select(t => Syntax.Token(t)).ToArray();
        private readonly SyntaxToken[] symbols = SyntaxToken.GetSymbols().Select(t => Syntax.Token(t))
            .OrderByDescending(s => s.Text.Length).ToArray();
    
        private readonly StringBuilder builder = new();
        private readonly TextView source;
        private readonly string document;

        private IEnumerator<SyntaxToken> enumerator;

        // Constructor
        public TokenParser(TextView textView, string document)
        {
            // Check for null
            if(textView == null)
                throw new ArgumentNullException(nameof(textView));

            this.source = textView;
            this.document = document;
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
            //List<SyntaxTrivia> trivia = new();

            // Read all tokens with minimal lookahead
            while(source.EOF == false)
            {
                // Get leading trivia
                //ReadLeadingTrivia(source.Position == 0, trivia);
                SyntaxTrivia[] leadingTrivia = ReadTrivia(source.Position == 0, true)
                    .ToArray();

                // Try to get last or default trivia
                SyntaxTrivia? lastLeadingTrivia = leadingTrivia.LastOrDefault();

                //// Get leading trivia string immediately before the token - used for toking parser separation
                //string leadingTriviaString = trivia.Count > 0
                //    ? trivia[trivia.Count - 1].Text
                //    : string.Empty;

                // Check for eof after trivia
                if(source.EOF == true)
                {
                    // Return eof token with trivia
                    yield return Syntax.Token(SyntaxTokenKind.EOF)
                        .WithLeadingTrivia(leadingTrivia);
                    yield break;
                }


                // ### LITERAL - Check for literal string
                else if (MatchLiteral(out SyntaxToken literal) == true)
                {
                    // Read end trivia
                    //ReadTrailingTrivia(trivia);

                    // Get the literal
                    //yield return previous = trivia.Count > 0
                    //    ? literal.WithTrivia(trivia)
                    //    : literal;
                    yield return previous = literal
                        .WithLeadingTrivia(leadingTrivia)
                        .WithTrailingTrivia(ReadTrivia(false, false));
                }
                // ### KEYWORD - Check for keyword delimited by whitespace or symbol
                else if (MatchKeyword(lastLeadingTrivia, previous, out SyntaxToken keyword) == true)
                {
                    //// Read end trivia
                    //ReadTrailingTrivia(trivia);

                    //// Get the keyword
                    //yield return previous = trivia.Count > 0
                    //    ? keyword.WithTrivia(trivia)
                    //    : keyword;
                    yield return previous = keyword
                        .WithLeadingTrivia(leadingTrivia)
                        .WithTrailingTrivia(ReadTrivia(false, false));
                }
                // ### SYMBOL - Check for symbol delimited by whitespace, letter or digit
                else if (MatchSymbol(lastLeadingTrivia, previous, out SyntaxToken symbol) == true)
                {
                    //// Read end trivia
                    //ReadTrailingTrivia(trivia);

                    //// Get the symbol
                    //yield return previous = trivia.Count > 0
                    //    ? symbol.WithTrivia(trivia)
                    //    : symbol;
                    yield return previous = symbol
                        .WithLeadingTrivia(leadingTrivia)
                        .WithTrailingTrivia(ReadTrivia(false, false));
                }
                // ### NUMBER - Check for numeric value delimited by whitespace or letter
                else if (MatchNumber(lastLeadingTrivia, previous, out SyntaxToken number, out SyntaxToken descriptor) == true)
                {
                    //// Read end trivia
                    //ReadTrailingTrivia(trivia);

                    //// Get the symbol
                    //yield return previous = trivia.Count > 0
                    //    ? number.WithTrivia(trivia.Where(t => t.IsLeading))
                    //    : number;
                    yield return previous = number
                        .WithLeadingTrivia(leadingTrivia);

                    // Check for descriptor
                    if(descriptor.Kind != SyntaxTokenKind.Invalid)
                        yield return previous = descriptor
                            .WithTrailingTrivia(ReadTrivia(false, false));
                }
                // ### IDENTIFIER - Check for identifier name
                else if (MatchIdentifier(lastLeadingTrivia, previous, out SyntaxToken identifier) == true)
                {
                    //// Read end trivia
                    //ReadTrailingTrivia(trivia);

                    //// Get the symbol
                    //yield return previous = trivia.Count > 0
                    //    ? identifier.WithTrivia(trivia)
                    //    : identifier;

                    yield return previous = identifier
                        .WithLeadingTrivia(leadingTrivia)
                        .WithTrailingTrivia(ReadTrivia(false, false));
                }
                
                // ### UNKNOWN - could not parse the string
                else
                {
                    // Try to recover from parser error
                    throw new Exception("Parser error: " + source.Peek());
                }

                // Recycle trivia list
                //trivia.Clear();
            }

            // Return eof with no trivia
            yield return Syntax.Token(SyntaxTokenKind.EOF);
        }

        #region Trivia
        private IEnumerable<SyntaxTrivia> ReadTrivia(bool isFirstToken, bool isLeading)
        {            
            while(source.EOF == false)
            {
                // Get current position
                int position = source.Position;

                // Check for new line
                if (ReadEndLineTrivia(out SyntaxTrivia newLine) == true)
                {
                    // Get the new line
                    yield return newLine;

                    // Check for trailing
                    if (isFirstToken == false && isLeading == false)
                        yield break;
                }

                // Check for whitespace
                if (ReadWhitespaceTrivia(out SyntaxTrivia ws) == true)
                    yield return ws;

                // Check for position not changed
                if (source.Position == position)
                    break;
            }
        }

        private bool ReadEndLineTrivia(out SyntaxTrivia trivia)
        {
            // Check for end line with return
            if (source.Peek() == '\r' && source.Peek(1) == '\n')
            {
                // Read the full new line
                builder.Append(source.Consume());
                builder.Append(source.Consume());

                // Build the trivia
                trivia = Syntax.Trivia(SyntaxTriviaKind.Newline, builder.ToString());
                builder.Clear();
                return true;
            }
            else if(source.Peek() == '\n' || source.Peek() == '\r')
            {
                char current = source.Consume();

                // Finally build the trivia
                trivia = Syntax.Trivia(SyntaxTriviaKind.Newline, current.ToString());
                return true;
            }
            trivia = default;
            return false;
        }

        private bool ReadWhitespaceTrivia(out SyntaxTrivia trivia)
        {
            // Get current char
            char current = source.Peek();

            // Check for whitespace
            if (IsWhiteSpaceTrivia(current) == true)
            {
                builder.Append(current);
                source.Consume();

                // Continue reading whitespace
                current = source.Peek();

                while (IsWhiteSpaceTrivia(current) == true)
                {
                    builder.Append(current);
                    source.Consume();

                    // Get next
                    current = source.Peek();
                }

                // Finally build the trivia
                trivia = Syntax.Trivia(SyntaxTriviaKind.Whitespace, builder.ToString());
                builder.Clear();
                return true;
            }
            trivia = default;
            return false;
        }

        //private void ReadLeadingTrivia(bool isStart, IList<SyntaxTrivia> trivia)
        //{
        //    while (source.EOF == false)
        //    {
        //        Get current char
        //        char current = source.Peek();

        //        Check for leading whitespace
        //        if (IsWhiteSpaceTrivia(current) == true)
        //            {
        //                builder.Append(current);
        //                source.Consume();

        //                Check for build the string
        //               if (IsWhiteSpaceTrivia(source.Peek()) == false)
        //                    {
        //                        We can build a whitespace trivia object
        //                trivia.Add(Syntax.Trivia(SyntaxTriviaKind.Whitespace, builder.ToString()));
        //                        builder.Clear();
        //                    }
        //            }
        //        Check for any other whitespace for starting token only
        //        else if (isStart == true && IsEndLineTrivia(current) == true)
        //                    {
        //                        builder.Append(current);
        //                        source.Consume();

        //                        Check for build the string
        //                       if (IsEndLineTrivia(source.Peek()) == false)
        //                            {
        //                                We can build an end line trivia object
        //                trivia.Add(Syntax.Trivia(SyntaxTriviaKind.Newline, builder.ToString()));
        //                                builder.Clear();
        //                            }
        //                    }
        //                    else
        //                    {
        //                        Must be some other token to be parsed
        //                        break;
        //                    }
        //    }
        //}

        //private void ReadTrailingTrivia(IList<SyntaxTrivia> trivia)
        //{
        //    First lookahead to see if there is an end line - if not then we should not parse the trailing trivia
        //    int position = 0;
        //    while (source.EOF == false)
        //    {
        //        Peek current
        //        char current = source.Peek(position++);

        //        Check for end line or end of stream
        //        if (IsEndLineTrivia(current) == true || current == '\0')
        //                break;

        //        Handle whitespace
        //        if (IsWhiteSpaceTrivia(current) == true)
        //            continue;

        //        Must be some other character that should be parsed as a token
        //        return;
        //    }

        //    while (source.EOF == false)
        //    {
        //        Get current char
        //        char current = source.Peek();

        //        Check for whitespace
        //        if (IsWhiteSpaceTrivia(current) == true)
        //            {
        //                builder.Append(current);
        //                source.Consume();

        //                Check for build string
        //               if (IsWhiteSpaceTrivia(source.Peek()) == false)
        //                    {
        //                        trivia.Add(Syntax.Trivia(SyntaxTriviaKind.Whitespace, builder.ToString()));
        //                        builder.Clear();
        //                    }
        //            }
        //        Check for end line
        //        else if (IsEndLineTrivia(current) == true)
        //                {
        //                    builder.Append(current);
        //                    source.Consume();

        //                    Check for build string
        //                   if (IsEndLineTrivia(source.Peek()) == false)
        //                        {
        //                            trivia.Add(Syntax.Trivia(SyntaxTriviaKind.Newline, builder.ToString()));
        //                            builder.Clear();
        //                        }
        //                }
        //                else
        //                {
        //                    Must be end line or some other token to be parsed
        //                    break;
        //                }
        //    }
        //}

        private bool IsLineCommentTrivia(out string lineCommentText)
        {
            // Check for line comment
            if (MatchString(source, SyntaxTrivia.LineComment) == true)
            {
                // Create comment builder
                StringBuilder commentBuilder = new StringBuilder();

                // Consume those characters
                source.Consume(SyntaxTrivia.LineComment.Length);

                // Append comment start
                commentBuilder.Append(SyntaxTrivia.LineComment);

                // Append until line end
                while(source.EOF == false && IsEndLineTrivia(source.Peek()) == false)
                {
                    // Consume and append
                    commentBuilder.Append(source.Consume());
                }

                // Get the line comment text
                lineCommentText = commentBuilder.ToString();
                return true;
            }
            lineCommentText = null;
            return false;
        }

        //private bool IsBlockCommentTrivia(out string blockCommentText)
        //{

        //}

        private bool IsWhiteSpaceTrivia(char c)
        {
            return char.IsWhiteSpace(c) == true || c == '\t';
        }

        private bool IsEndLineTrivia(char c)
        {
            return c == '\r' || c == '\n';
        }
        #endregion

        private bool MatchLiteral(out SyntaxToken literal)
        {
            // Check for quote
            if (source.Peek() == '"')
            {
                // Get starting location
                SyntaxLocation start = source.GetLocation();

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

                // Get end location
                SyntaxLocation end = source.GetLocation();

                // Build the literal
                literal = new SyntaxToken(SyntaxTokenKind.Literal, builder.ToString(), 
                    new SyntaxSpan(document, start, end));

                // Recycle builder
                builder.Clear();
                return true;
            }

            literal = default;
            return false;
        }

        private bool MatchKeyword(SyntaxTrivia? lastLeadingTrivia, in SyntaxToken previousToken, out SyntaxToken keyword)
        {
            // Check for start of doc, white space, end of block comment, or delimiting symbol to start the keyword
            if (source.Position > 0 
                && lastLeadingTrivia == null 
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
                        && MatchString(source, SyntaxTrivia.LineComment, length) == false 
                        && MatchString(source, SyntaxTrivia.BlockCommentStart, length) == false 
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

        private bool MatchSymbol(SyntaxTrivia? lastLeadingTrivia, in SyntaxToken previousToken, out SyntaxToken symbol)
        {
            // Check for start of doc, white space, end of block comment, or delimiting symbol to start the keyword
            if (source.Position > 0
                && lastLeadingTrivia == null
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
                        && MatchString(source, SyntaxTrivia.LineComment, length) == false
                        && MatchString(source, SyntaxTrivia.BlockCommentStart, length) == false
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

        private bool MatchNumber(SyntaxTrivia? lastLeadingTrivia, in SyntaxToken previousToken, out SyntaxToken number, out SyntaxToken descriptor)
        {
            descriptor = default;

            // Require whitespace or symbol before number
            if (source.Position > 0
                && lastLeadingTrivia == null
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
                // Get start
                SyntaxLocation start = source.GetLocation();

                // Consume first value
                builder.Append(source.Consume());

                // Check for decimal point consumed
                bool consumedDecimal = first == '.';

                // Consume all remaining digits
                while(char.IsDigit(source.Peek()) == true || (consumedDecimal == false && source.Peek() == '.'))
                {
                    // Set flag
                    if (source.Peek() == '.')
                    {
                        consumedDecimal = true;

                        // Check for no trailing number after dot - should be treated as member access so process as a separate token
                        if (char.IsDigit(source.Peek(1)) == false)
                            break;
                    }

                    // Append the character
                    builder.Append(source.Consume());
                }

                // Get end
                SyntaxLocation end = source.GetLocation();

                // Build the full number string
                number = new SyntaxToken(SyntaxTokenKind.Literal, builder.ToString(),
                    new SyntaxSpan(document, start, end));

                // Check for descriptor
                // Unsigned
                if(source.Peek() == 'U' || source.Peek() == 'u')
                {
                    // Get start
                    start = source.GetLocation();

                    // Check for following L
                    if(source.Peek(1) == 'L' || source.Peek(1) == 'l')
                    {
                        source.Consume(2);
                        descriptor = new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "UL",
                            new SyntaxSpan(document, start, source.GetLocation()));
                    }
                    else
                    {
                        source.Consume();
                        descriptor = new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "U",
                            new SyntaxSpan(document, start, source.GetLocation()));
                    }
                }
                // Long
                else if(source.Peek() == 'L' || source.Peek() == 'l')
                {
                    // Get start
                    start = source.GetLocation();

                    source.Consume();
                    descriptor = new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "L",
                        new SyntaxSpan(document, start, source.GetLocation()));
                }
                // Float
                else if(source.Peek() == 'F' || source.Peek() == 'f')
                {
                    // Get start
                    start = source.GetLocation();

                    source.Consume();
                    descriptor = new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "F",
                        new SyntaxSpan(document, start, source.GetLocation()));
                }
                // Double
                else if(source.Peek() == 'D' || source.Peek() == 'd')
                {
                    // Get start
                    start = source.GetLocation();

                    source.Consume();
                    descriptor = new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "D",
                        new SyntaxSpan(document, start, source.GetLocation()));
                }

                // Recycle builder
                builder.Clear();
                return true;
            }

            // Not a number
            number = default;
            return false;
        }

        private bool MatchIdentifier(SyntaxTrivia? lastLeadingTrivia, in SyntaxToken previousToken, out SyntaxToken identifier)
        {
            // Require whitespace or symbol before identifier
            if (source.Position > 0
                && lastLeadingTrivia == null
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
                // Get start
                SyntaxLocation start = source.GetLocation();

                // Read thr first character
                builder.Append(source.Consume());

                // Read all supported characters
                while(source.EOF == false
                    && (char.IsLetterOrDigit(source.Peek()) == true || source.Peek() == '_'))
                {
                    // Append the character
                    builder.Append(source.Consume());
                }

                // Get end
                SyntaxLocation end = source.GetLocation();

                // Create the token
                identifier = new SyntaxToken(SyntaxTokenKind.Identifier, builder.ToString(),
                    new SyntaxSpan(document, start, end));

                // Recycle builder
                builder.Clear();
                return true;
            }

            // Not an identifier
            identifier = default;
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
