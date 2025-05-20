
namespace LumaSharp.Compiler.AST
{
    public sealed class ContinueStatementSyntax : StatementSyntax
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
        internal ContinueStatementSyntax()
            : this(
                  new SyntaxToken(SyntaxTokenKind.ContinueKeyword),
                  new SyntaxToken(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal ContinueStatementSyntax(SyntaxToken keyword, SyntaxToken semicolon)
            : base(semicolon)
        {
            // Check kind
            if (keyword.Kind != SyntaxTokenKind.ContinueKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.ContinueKeyword);

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
