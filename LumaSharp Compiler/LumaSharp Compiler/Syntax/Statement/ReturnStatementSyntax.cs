using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ReturnStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly ExpressionSyntax expression;
        private readonly SyntaxToken semicolon;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return semicolon; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        public ExpressionSyntax Expression
        {
            get { return expression; }
        }

        public bool HasExpression
        {
            get { return expression != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                if (HasExpression == true)
                    yield return expression;
            }
        }

        // Constructor
        internal ReturnStatementSyntax(SyntaxToken keyword, ExpressionSyntax expression, SyntaxToken semicolon)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.ReturnKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.ReturnKeyword);

            // Check kind
            if (semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);


            this.keyword = keyword;
            this.expression = expression;
            this.semicolon = semicolon;

            // Set parent
            if(expression != null) expression.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitReturnStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitReturnStatement(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Return statement
            if(HasExpression == true)
            {
                expression.GetSourceText(writer);
            }

            // Semicolon
            semicolon.GetSourceText(writer);
        }
    }

    public static partial class Syntax
    {
        // Methods
        public static ReturnStatementSyntax Return(ExpressionSyntax expression = null) 
            => new (Token(SyntaxTokenKind.ReturnKeyword), expression, Token(SyntaxTokenKind.SemicolonSymbol));
        public static ReturnStatementSyntax Return(SyntaxToken keyword, SyntaxToken semicolon, ExpressionSyntax expression = null) 
            => new (keyword, expression, semicolon);
    }
}
