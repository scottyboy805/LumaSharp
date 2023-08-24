
namespace LumaSharp_Compiler.Syntax
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

        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        // Constructor
        internal ContinueStatementSyntax()
            : base(new SyntaxToken(";"))
        {
            keyword = new SyntaxToken("continue");
        }


        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            writer.Write(keyword.ToString());
            writer.Write(statementEnd.ToString());
        }
    }
}
