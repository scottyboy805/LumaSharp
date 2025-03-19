
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
            get { return keyword; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        // Constructor
        internal BreakStatementSyntax(SyntaxNode parent)
            : base(parent)
        {
            this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.BreakKeyword);
        }

        internal BreakStatementSyntax(SyntaxNode parent, LumaSharpParser.StatementContext statement)
            : base(parent)
        {
            keyword = new SyntaxToken(SyntaxTokenKind.BreakKeyword, statement.BREAK());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            keyword.GetSourceText(writer);
        }
    }
}
