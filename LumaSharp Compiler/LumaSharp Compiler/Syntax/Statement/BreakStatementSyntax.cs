
namespace LumaSharp_Compiler.AST
{
    public sealed class BreakStatementSyntax : StatementSyntax
    {
        // Private
        private SyntaxToken keyword = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        // Constructor
        internal BreakStatementSyntax()
            : base(new SyntaxToken("break"))
        {
            this.keyword = base.StartToken;
        }

        internal BreakStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.StatementContext statement)
            : base(tree, parent, statement)
        {
            keyword = new SyntaxToken(statement.BREAK());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            keyword.GetSourceText(writer);
            statementEnd.GetSourceText(writer);
        }
    }
}
