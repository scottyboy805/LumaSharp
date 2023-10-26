
namespace LumaSharp_Compiler.AST
{
    public sealed class ContinueStatementSyntax : StatementSyntax
    {
        // Private
        private SyntaxToken keyword = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        // Constructor
        internal ContinueStatementSyntax()
            : base(new SyntaxToken("continue"))
        {
            this.keyword = base.StartToken;
        }

        internal ContinueStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.StatementContext statement)
            : base(tree, parent, statement)
        {
            keyword = new SyntaxToken(statement.CONTINUE());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            writer.Write(keyword.ToString());
            writer.Write(statementEnd.ToString());
        }
    }
}
