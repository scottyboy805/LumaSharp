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

            // Read all tokens with minimal lookahead
            while(source.EOF == false)
            {
                // Get leading trivia
                string leadingTrivia = ReadLeadingTrivia(source.Position == 0);

                // ### COMMENT - Check for line or block comment
                if (MatchCommentStart(out SyntaxToken commentStart) == true)
                {
                    // Get the comment start
                    yield return commentStart;

                    // Get the comment text
                    yield return MatchCommentText(commentStart.Kind == SyntaxTokenKind.LineComment
                            ? Environment.NewLine
                            : blockCommentEnd);

                    // Get the comment end
                    if (commentStart.Kind == SyntaxTokenKind.BlockCommentStart)
                        yield return previous = MatchCommentEnd(blockCommentEnd);
                }
                // ### LITERAL - Check for literal string
                else if (MatchLiteral(out SyntaxToken literal) == true)
                {
                    // Get the literal
                    yield return previous = literal;
                }
                // ### KEYWORD - Check for keyword delimited by whitespace or symbol
                else if (MatchKeyword(leadingTrivia, previous, out SyntaxToken keyword) == true)
                {
                    // Get the keyword
                    yield return previous = keyword;
                }
                // ### SYMBOL - Check for symbol delimited by whitespace, letter or digit
                else if (MatchSymbol(leadingTrivia, previous, out SyntaxToken symbol) == true)
                {
                    // Get the symbol
                    yield return previous = symbol;
                }
                // ### NUMBER - Check for numeric value delimited by whitespace or letter
                else if (MatchNumber(leadingTrivia, previous, out SyntaxToken number) == true)
                {
                    // Get the symbol
                    yield return previous = number;
                }
                // ### IDENTIFIER - Check for identifier name
                else if (MatchIdentifier(leadingTrivia, previous, out SyntaxToken identifier) == true)
                {
                    // Get the symbol
                    yield return previous = identifier;
                }
                
                // ### UNKNOWN - could not parse the string
                else
                {
                    // Try to recover from parser error
                    throw new Exception("Parser error: " + source.Peek());
                }
            }
        }

        #region Trivia
        private string ReadLeadingTrivia(bool isStart)
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
                }
                // Check for any other whitespace for starting token only
                else if(isStart == true && IsEndLineTrivia(current) == true)
                {
                    builder.Append(current);
                    source.Consume();
                }
                else
                {
                    // Must be some other token to be parsed
                    break;
                }
            }

            // Check for any
            if (builder.Length > 0)
            {
                // Get the trivia
                string trivia = builder.ToString();

                // Recycle the builder
                builder.Clear();
                return trivia;
            }
            return null;
        }

        private string ReadTrailingTrivia()
        {
            while (source.EOF == false)
            {
                // Get current char
                char current = source.Peek();

                // Check for whitespace or end line
                if(IsWhiteSpaceTrivia(current) == true || IsEndLineTrivia(current) == true)
                {
                    builder.Append(current);
                    source.Consume();
                }
                else
                {
                    // Must be end line or some other token to be parsed
                    break;
                }
            }

            // Check for any
            if(builder.Length > 0)
            {
                // Get the trivia
                string trivia = builder.ToString();

                // Recycle the builder
                builder.Clear();
                return trivia;
            }
            return null;
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

        private bool MatchNumber(string leadingTrivia, in SyntaxToken previousToken, out SyntaxToken number)
        {
            // Require whitespace or symbol before number
            if(source.Position > 0
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
