
using System.Collections;

namespace LumaSharp.Compiler.AST
{
    public class SeparatedTokenList : SyntaxNode, IEnumerable<SyntaxToken>
    {
        // Type
        private struct TokenSeparatedElement
        {
            // Public
            public SyntaxToken separator;
            public SyntaxToken token;
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
                    return tokenList[0].token;

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
                    return tokenList[tokenList.Count - 1].token;

                // Not valid
                return SyntaxToken.Invalid;
            }
        }

        public SyntaxToken this[int index]
        {
            get 
            {
                // Check bounds
                if (index >= tokenList.Count)
                    throw new IndexOutOfRangeException();

                return tokenList[index].token; 
            }
        }

        public int Count
        {
            get { return tokenList.Count; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal SeparatedTokenList(SyntaxNode parent, SyntaxTokenKind separatorKind, SyntaxTokenKind? tokenKind)
            : base(parent)
        {
            this.separatorKind = separatorKind;
            this.tokenKind = tokenKind;
        }

        internal SeparatedTokenList(SyntaxNode parent, SyntaxToken[] items, SyntaxTokenKind separatorKind, SyntaxTokenKind? tokenKind)
            : this(parent, separatorKind, tokenKind)
        {
            // Add all with separator
            foreach (SyntaxToken item in items)
                AddElement(item, new SyntaxToken(separatorKind));
        }

        internal SeparatedTokenList(SyntaxNode parent, LumaSharpParser.NamespaceNameContext name)
            : base(parent)
        {
            this.separatorKind = SyntaxTokenKind.ColonSymbol;
            this.tokenKind = SyntaxTokenKind.Identifier;

            // Add identifier
            AddElement(new SyntaxToken(SyntaxTokenKind.Identifier, name.IDENTIFIER()), null);

            // Add secondary
            LumaSharpParser.NamespaceNameSecondaryContext[] secondaryNamespaceNames = name.namespaceNameSecondary();

            // Check for valid
            if (secondaryNamespaceNames != null)
            {
                foreach (LumaSharpParser.NamespaceNameSecondaryContext secondaryNamespaceName in secondaryNamespaceNames)
                {
                    AddElement(
                        new SyntaxToken(SyntaxTokenKind.Identifier, secondaryNamespaceName.IDENTIFIER()),
                        new SyntaxToken(SyntaxTokenKind.ColonSymbol, secondaryNamespaceName.COLON()));
                }
            }
        }

        // Methods
        public void AddElement(SyntaxToken tokenElement, SyntaxToken? separator)
        {
            // Check kind
            if(separator != null && separator.Value.Kind != separatorKind)
                throw new ArgumentException("Separator must be of kind: " + separatorKind);

            // Check for count
            if (tokenList.Count > 0 && separator == null)
                throw new ArgumentException("Separator must be provided for non-zero indexed elements");

            // Check for token
            if(tokenKind != null && tokenElement.Kind != tokenKind.Value)
                throw new ArgumentException("Token must be of kind: " +  tokenKind);

            // Add to list
            tokenList.Add(new TokenSeparatedElement
            {
                separator = separator != null ? separator.Value : default,
                token = tokenElement,
            });
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Process all elements
            for (int i = 0; i < tokenList.Count; i++)
            {
                // Check for token
                if (i > 0)
                    tokenList[i].separator.GetSourceText(writer);

                // Get syntax source
                tokenList[i].token.GetSourceText(writer);
            }
        }

        public IEnumerator<SyntaxToken> GetEnumerator()
        {
            foreach(TokenSeparatedElement token in tokenList)
                yield return token.token;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
