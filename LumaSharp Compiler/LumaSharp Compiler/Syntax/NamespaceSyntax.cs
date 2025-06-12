
using LumaSharp.Compiler.AST.Visitor;

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
        internal NamespaceSyntax(SeparatedTokenList name)
            : this(
                  Syntax.Token(SyntaxTokenKind.NamespaceKeyword),
                  name)
        {
        }

        internal NamespaceSyntax(SyntaxToken keyword, SeparatedTokenList name)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.NamespaceKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.NamespaceKeyword);

            // Check for null
            if(name == null)
                throw new ArgumentNullException(nameof(name));

            this.keyword = keyword;
            this.name = name;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitNamespace(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            keyword.GetSourceText(writer);

            // Write namespace name
            name.GetSourceText(writer);
        }
    }
}
