
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class AccessorLambdaSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken lambda;
        private readonly ExpressionSyntax expression;
        private readonly SyntaxToken semicolon;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lambda; }
        }

        public override SyntaxToken EndToken
        {
            get { return semicolon; }
        }

        public SyntaxToken Lambda
        {
            get { return lambda; }
        }

        public ExpressionSyntax Expression
        {
            get { return expression; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return expression; }
        }

        // Constructor
        internal AccessorLambdaSyntax(ExpressionSyntax expression)
            : this(
                  Syntax.Token(SyntaxTokenKind.LambdaSymbol),
                  expression,
                  Syntax.Token(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal AccessorLambdaSyntax(SyntaxToken lambda, ExpressionSyntax expression, SyntaxToken semicolon)
        {
            // Check kind
            if (lambda.Kind != SyntaxTokenKind.LambdaSymbol)
                throw new ArgumentException(nameof(lambda) + " must be of kind: " + SyntaxTokenKind.LambdaSymbol);

            if(semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);

            // Check null
            if(expression == null)
                throw new ArgumentNullException(nameof(expression));

            this.lambda = lambda;
            this.expression = expression;
            this.semicolon = semicolon;

            // Set parent
            expression.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitAccessorLambda(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitAccessorLambda(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Lambda
            lambda.GetSourceText(writer);

            // Expression
            expression.GetSourceText(writer);

            // Semicolon
            semicolon.GetSourceText(writer);
        }
    }
}
