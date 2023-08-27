
namespace LumaSharp_Compiler.Syntax
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
        internal BreakStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.StatementContext statement)
            : base(tree, parent, statement)
        {
            keyword = new SyntaxToken(statement.BREAK());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            writer.Write(keyword.ToString());
            writer.Write(statementEnd.ToString());
        }
    }
}
