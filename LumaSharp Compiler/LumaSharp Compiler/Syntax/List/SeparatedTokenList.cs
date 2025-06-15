using LumaSharp.Compiler.AST.Visitor;
using System.Collections;

namespace LumaSharp.Compiler.AST
{
    public class SeparatedTokenList : SyntaxNode, IEnumerable<SyntaxToken>
    {
        // Type
        public readonly struct TokenSeparatedElement
        {
            // Public            
            public readonly SyntaxToken Token;
            public readonly SyntaxToken? Separator;

            // Constructor
            public TokenSeparatedElement(SyntaxToken token, SyntaxToken? separator)
            {
                this.Token = token;
                this.Separator  = separator;
            }
        }

        // Private
        private readonly List<TokenSeparatedElement> tokenList = new();
        private readonly SyntaxTokenKind separatorKind;
        private readonly SyntaxTokenKind? tokenKind;

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                // Check for any
                if (tokenList.Count > 0)
                    return tokenList[0].Token;

                // Not valid
                return SyntaxToken.Invalid;
            }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for any
                if(tokenList.Count > 0)
                    return tokenList[tokenList.Count - 1].Token;

                // Not valid
                return SyntaxToken.Invalid;
            }
        }

        public SyntaxTokenKind SeparatorKind
        {
            get { return separatorKind; }
        }

        public SyntaxTokenKind? TokenKind
        {
            get { return tokenKind; }
        }

        public SyntaxToken this[int index]
        {
            get 
            {
                // Check bounds
                if (index >= tokenList.Count)
                    throw new IndexOutOfRangeException();

                return tokenList[index].Token; 
            }
        }

        public IReadOnlyList<TokenSeparatedElement> Elements
        {
            get { return tokenList; }
        }

        public int Count
        {
            get { return tokenList.Count; }
        }

        public bool HasTrailingSeparator
        {
            get
            {
                if (tokenList.Count > 0)
                    return tokenList[tokenList.Count - 1].Separator != null;

                return false;
            }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal SeparatedTokenList(SyntaxTokenKind separatorKind, SyntaxToken tokenSingle)
        {
            this.separatorKind = separatorKind;
            this.tokenList = new();
            this.tokenList.Add(new TokenSeparatedElement(tokenSingle, null));
            this.tokenKind = tokenSingle.Kind;
        }

        internal SeparatedTokenList(SyntaxTokenKind separatorKind, IEnumerable<SyntaxToken> elements, SyntaxTokenKind? tokenKind)
            : this(separatorKind, BuildSeparatedTokens(separatorKind, elements), tokenKind)
        {
        }

        internal SeparatedTokenList(SyntaxTokenKind separatorKind, IEnumerable<TokenSeparatedElement> elements, SyntaxTokenKind? tokenKind)
        {
            this.separatorKind = separatorKind;
            this.tokenList = new(elements != null
                ? elements.Count()
                : 0);
            this.tokenKind = tokenKind;

            // Check for any
            if(elements != null)
            {
                // Add all elements
                int current = 0;
                foreach(TokenSeparatedElement item in elements)
                {
                    // Add the element
                    this.tokenList.Add(item);

                    // Check kind
                    if (tokenKind != null && item.Token.Kind != tokenKind)
                        throw new ArgumentException("Token element must be of kind: " + tokenKind.Value);

                    // Expect separator?
                    if (current < tokenList.Count - 1 && item.Separator == null)
                        throw new ArgumentException("A separator must be provided when token continues at index: " + current);

                    // Check kind
                    if (item.Separator != null && item.Separator.Value.Kind != separatorKind)
                        throw new ArgumentException("Separator must be of kind: " + separatorKind);

                    current++;
                }
            }
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitTokenList(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitTokenList(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Process all elements
            for (int i = 0; i < tokenList.Count; i++)
            {
                // Get syntax source
                tokenList[i].Token.GetSourceText(writer);

                // Check for token
                if (i < tokenList.Count - 1)
                    tokenList[i].Separator?.GetSourceText(writer);
            }

            // Check for trailing separator
            if (HasTrailingSeparator == true)
                tokenList[tokenList.Count - 1].Separator?.GetSourceText(writer);
        }

        public IEnumerator<SyntaxToken> GetEnumerator()
        {
            foreach(TokenSeparatedElement token in tokenList)
                yield return token.Token;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static IEnumerable<TokenSeparatedElement> BuildSeparatedTokens(SyntaxTokenKind separatorKind, IEnumerable<SyntaxToken> tokens)
        {
            // Check for null
            if (tokens == null)
                yield break;

            // Get count
            int count = tokens.Count();
            int current = 0;

            // Get all
            foreach(SyntaxToken token in tokens)
            {
                // Create the separator
                SyntaxToken? separator = current++ < count - 1
                    ? Syntax.Token(separatorKind)
                    : (SyntaxToken?)null;

                // Create the element
                yield return new TokenSeparatedElement(token, separator);
            }
        }
    }
}
