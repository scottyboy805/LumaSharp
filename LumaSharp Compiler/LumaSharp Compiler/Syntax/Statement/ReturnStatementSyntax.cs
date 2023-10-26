
namespace LumaSharp_Compiler.AST
{
    public sealed class ReturnStatementSyntax : StatementSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private ExpressionSyntax returnExpression = null;
        private SyntaxToken semicolon = null;

        // Properties
        public SyntaxToken Keyword
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
        internal ReturnStatementSyntax(ExpressionSyntax expression)
            : base(SyntaxToken.Return())
        {
            this.keyword = base.StartToken;
            this.returnExpression = expression;
            this.semicolon = SyntaxToken.Semi();

            // Check for expression
            if (expression != null)
                keyword.WithTrailingWhitespace(" ");
        }

        internal ReturnStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ReturnStatementContext statement)
            : base(tree, parent, statement)
        {
            // Keyword
            this.keyword = new SyntaxToken(statement.RETURN());

            // Expression
            if (statement.expression() != null)
                this.returnExpression = ExpressionSyntax.Any(tree, this, statement.expression());

            // Semicolon
            this.semicolon = new SyntaxToken(statement.semi);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Return statement
            if(HasReturnExpression == true)
            {
                returnExpression.GetSourceText(writer);
            }

            // End statement
            semicolon.GetSourceText(writer);

        }
    }
}
