
namespace LumaSharp.Compiler.AST
{
    public sealed class TernaryExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly ExpressionSyntax condition;
        private readonly ExpressionSyntax trueExpression;
        private readonly ExpressionSyntax falseExpression;
        private readonly SyntaxToken ternary;
        private readonly SyntaxToken colon;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return condition.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return falseExpression.EndToken; }
        }

        public ExpressionSyntax Condition
        {
            get { return condition; }
        }

        public ExpressionSyntax TrueExpression
        {
            get { return trueExpression; }
        }

        public ExpressionSyntax FalseExpression
        {
            get { return falseExpression; }
        }

        public SyntaxToken Ternary
        {
            get { return ternary; }
        }

        public SyntaxToken Colon
        {
            get { return colon; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return condition;
                yield return trueExpression;
                yield return falseExpression;
            }
        }

        // Constructor
        internal TernaryExpressionSyntax(SyntaxNode parent, ExpressionSyntax condition, ExpressionSyntax trueExpression, ExpressionSyntax falseExpression)
            : base(parent, null)
        {
            this.condition = condition;
            this.trueExpression = trueExpression;
            this.falseExpression = falseExpression;

            ternary = Syntax.KeywordOrSymbol(SyntaxTokenKind.TernarySymbol);
            colon = Syntax.KeywordOrSymbol(SyntaxTokenKind.ColonSymbol);
        }

        internal TernaryExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            // Condition
            this.condition = Any(this, expression.expression(0));

            // True expression
            this.trueExpression = Any(this, expression.expression(1));

            // False expression
            this.falseExpression = Any(this, expression.expression(2));

            ternary = new SyntaxToken(SyntaxTokenKind.TernarySymbol, expression.TERNARY());
            colon = new SyntaxToken(SyntaxTokenKind.ColonSymbol, expression.COLON());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write condition
            condition.GetSourceText(writer);

            // Ternary
            ternary.GetSourceText(writer);

            // Write true
            trueExpression.GetSourceText(writer);

            // Write alternate
            colon.GetSourceText(writer);

            // Write false
            falseExpression.GetSourceText(writer);
        }
    }
}
