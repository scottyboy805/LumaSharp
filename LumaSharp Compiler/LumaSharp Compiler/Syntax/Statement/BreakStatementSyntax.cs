
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

        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        // Constructor
        internal BreakStatementSyntax(SyntaxTree tree, SyntaxNode parent)
            : base(tree, parent, new SyntaxToken(";"))
        {
            keyword = new SyntaxToken("break");
        }


        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            writer.Write(keyword.ToString());
            writer.Write(statementEnd.ToString());
        }
    }
}
