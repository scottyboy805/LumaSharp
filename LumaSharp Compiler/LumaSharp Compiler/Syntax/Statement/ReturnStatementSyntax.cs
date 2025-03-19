
namespace LumaSharp.Compiler.AST
{
    public sealed class ReturnStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly SeparatedListSyntax<ExpressionSyntax> expressions;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for returns
                if (HasExpressions == true)
                    return expressions.EndToken;

                return keyword;
            }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public SeparatedListSyntax<ExpressionSyntax> Expressions
        {
            get { return expressions; }
        }

        public bool HasExpressions
        {
            get { return expressions != null; }
        }

        // Constructor
        internal ReturnStatementSyntax(SyntaxNode parent, ExpressionSyntax[] expressions)
            : base(parent)
        {
            this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.ReturnKeyword);

            // Add return expressions
            if (expressions != null)
            {
                // Create expressions
                this.expressions = new SeparatedListSyntax<ExpressionSyntax>(parent, SyntaxTokenKind.CommaSymbol);

                // Add all
                for (int i = 0; i < expressions.Length; i++)
                {
                    // Add expression with comma
                    this.expressions.AddElement(expressions[i], Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol));
                }
            }
        }

        internal ReturnStatementSyntax(SyntaxNode parent, LumaSharpParser.ReturnStatementContext statement)
            : base(parent)
        {
            // Keyword
            this.keyword = new SyntaxToken(SyntaxTokenKind.ReturnKeyword, statement.RETURN());

            // Expression list
            LumaSharpParser.ExpressionListContext expressionList = statement.expressionList();

            // Check for expression list
            if(expressionList != null)
                this.expressions = ExpressionSyntax.List(this, expressionList);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Return statement
            if(HasExpressions == true)
            {
                expressions.GetSourceText(writer);
            }
        }
    }
}
