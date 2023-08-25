
namespace LumaSharp_Compiler.Syntax
{
    public sealed class ReturnStatementSyntax : StatementSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private ExpressionSyntax returnExpression = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public ExpressionSyntax ReturnExpression
        {
            get { return returnExpression; }
        }

        public bool HasReturnExpression
        {
            get { return returnExpression != null; }
        }

        // Constructor
        internal ReturnStatementSyntax(SyntaxTree tree, SyntaxNode parent, ExpressionSyntax returnExpression = null)
            : base(tree, parent, new SyntaxToken(";"))
        {
            this.keyword = new SyntaxToken("return");
            this.returnExpression = returnExpression;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            writer.Write(keyword.ToString());

            // Return statement
            if(HasReturnExpression == true)
            {
                returnExpression.GetSourceText(writer);
            }

            // End statement
            writer.Write(statementEnd.ToString());

        }
    }
}
