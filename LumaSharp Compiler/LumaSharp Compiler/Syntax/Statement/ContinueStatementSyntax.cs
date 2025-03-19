
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
            get { return keyword; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        // Constructor
        internal ContinueStatementSyntax(SyntaxNode parent)
            : base(parent)
        {
            this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.ContinueKeyword);
        }

        internal ContinueStatementSyntax(SyntaxNode parent, LumaSharpParser.StatementContext statement)
            : base(parent)
        {
            keyword = new SyntaxToken(SyntaxTokenKind.ContinueKeyword, statement.CONTINUE());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            keyword.GetSourceText(writer);
        }
    }
}
