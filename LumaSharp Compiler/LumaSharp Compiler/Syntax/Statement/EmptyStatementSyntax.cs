
namespace LumaSharp.Compiler.AST
{
    public sealed class EmptyStatementSyntax : StatementSyntax
    {
        // Properties
        public override SyntaxToken StartToken
        {
            get { return Semicolon; }
        }

        public override SyntaxToken EndToken
        {
            get { return Semicolon; }
        }

        // Constructor
        internal EmptyStatementSyntax()
            : base(new SyntaxToken(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal EmptyStatementSyntax(SyntaxToken semicolon)
            : base(semicolon)
        {
            // Check kind
            if (semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Get semicolon text
            Semicolon.GetSourceText(writer);
        }
    }
}
