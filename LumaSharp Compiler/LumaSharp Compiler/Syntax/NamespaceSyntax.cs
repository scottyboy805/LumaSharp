
namespace LumaSharp.Compiler.AST
{
    public sealed class NamespaceSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly SeparatedTokenList name;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return name.EndToken; }
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
        internal NamespaceSyntax(SyntaxNode parent, string[] identifiers)
            : base(parent)
        {
            this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.NamespaceKeyword);
            this.name = new SeparatedTokenList(this, SyntaxTokenKind.ColonSymbol, SyntaxTokenKind.Identifier);

            foreach (string identifier in identifiers)
                this.name.AddElement(Syntax.Identifier(identifier), Syntax.KeywordOrSymbol(SyntaxTokenKind.ColonSymbol));
        }

        internal NamespaceSyntax(SyntaxNode parent, LumaSharpParser.NamespaceDeclarationContext namespaceDef) 
            : base(parent)
        {
            // Get keyword
            this.keyword = new SyntaxToken(SyntaxTokenKind.NamespaceKeyword, namespaceDef.NAMESPACE());

            // Create name
            this.name = new SeparatedTokenList(this, namespaceDef.namespaceName());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            keyword.GetSourceText(writer);

            // Write namespace name
            name.GetSourceText(writer);
        }
    }
}
