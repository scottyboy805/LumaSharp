using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class NamespaceSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly SeparatedTokenList name;
        private readonly SyntaxToken semicolon;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return semicolon; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        public SeparatedTokenList Name
        {
            get { return name; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal NamespaceSyntax(SeparatedTokenList name)
            : this(
                  Syntax.Token(SyntaxTokenKind.NamespaceKeyword),
                  name,
                  Syntax.Token(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal NamespaceSyntax(SyntaxToken keyword, SeparatedTokenList name, SyntaxToken semicolon)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.NamespaceKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.NamespaceKeyword);

            if (semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);

            // Check for null
            if(name == null)
                throw new ArgumentNullException(nameof(name));

            this.keyword = keyword;
            this.name = name;
            this.semicolon = semicolon;

            // Set parent
            name.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitNamespace(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitNamespace(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            keyword.GetSourceText(writer);

            // Write namespace name
            name.GetSourceText(writer);

            // Semicolon
            semicolon.GetSourceText(writer);
        }
    }
}
