
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken lParen;
        private readonly SyntaxToken rParen;
        private readonly ExpressionSyntax expression;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lParen; }
        }

        public override SyntaxToken EndToken
        {
            get { return rParen; }
        }

        public SyntaxToken LParen
        {
            get { return lParen; }
        }

        public SyntaxToken RParen
        {
            get { return rParen; }
        }

        public ExpressionSyntax Expression
        {
            get { return expression; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return expression;
            }
        }

        // Constructor
        internal ParenthesizedExpressionSyntax(ExpressionSyntax expression)
            : this(
                  Syntax.Token(SyntaxTokenKind.LParenSymbol),
                  expression,
                  Syntax.Token(SyntaxTokenKind.RParenSymbol))
        {
        }                  

        internal ParenthesizedExpressionSyntax(SyntaxToken lParen, ExpressionSyntax expression, SyntaxToken rParen)
        {
            // Check paren
            if(lParen.Kind != SyntaxTokenKind.LParenSymbol)
                throw new ArgumentException(nameof(lParen) + " must be of kind: " + SyntaxTokenKind.LParenSymbol);

            if(rParen.Kind != SyntaxTokenKind.RParenSymbol)
                throw new ArgumentException(nameof(rParen) + " must be of kind: " + SyntaxTokenKind.RParenSymbol);

            // Check null
            if(expression == null)
                throw new ArgumentNullException(nameof(expression));

            this.lParen = lParen;
            this.rParen = rParen;
            this.expression = expression;

            // Set parent
            expression.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // LParen
            lParen.GetSourceText(writer);

            // Expression
            expression.GetSourceText(writer);

            // RParen
            rParen.GetSourceText(writer);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitParenthesizedExpression(this);
        }
    }
}
