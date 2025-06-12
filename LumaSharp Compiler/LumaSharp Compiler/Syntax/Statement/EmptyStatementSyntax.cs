

using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class EmptyStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken semicolon;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return semicolon; }
        }

        public override SyntaxToken EndToken
        {
            get { return semicolon; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal EmptyStatementSyntax()
            : this(
                  Syntax.Token(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal EmptyStatementSyntax(SyntaxToken semicolon)
        {
            // Check kind
            if (semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);

            this.semicolon = semicolon;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitEmptyStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitEmptyStatement(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Get semicolon text
            semicolon.GetSourceText(writer);
        }
    }
}
