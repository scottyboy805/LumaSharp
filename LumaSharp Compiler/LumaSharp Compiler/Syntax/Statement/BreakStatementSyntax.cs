
namespace LumaSharp.Compiler.AST
{
    public sealed class BreakStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return Semicolon; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        // Constructor
        internal BreakStatementSyntax()
            : this(
                  new SyntaxToken(SyntaxTokenKind.BreakKeyword),
                  new SyntaxToken(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal BreakStatementSyntax(SyntaxToken keyword, SyntaxToken semicolon)
            : base(semicolon)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.BreakKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.BreakKeyword);

            // Check kind
            if (semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);

            this.keyword = keyword;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            keyword.GetSourceText(writer);
        }
    }
}
