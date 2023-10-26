
namespace LumaSharp_Compiler.AST
{
    public sealed class TernaryExpressionSyntax : ExpressionSyntax
    {
        // Private
        private ExpressionSyntax condition = null;
        private ExpressionSyntax trueExpression = null;
        private ExpressionSyntax falseExpression = null;
        private SyntaxToken ternary = null;
        private SyntaxToken alternate = null;

        // Properties
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
        internal TernaryExpressionSyntax(ExpressionSyntax condition, ExpressionSyntax trueExpression, ExpressionSyntax falseExpression)
            : base(condition.StartToken, falseExpression.EndToken)
        {
            this.condition = condition;
            this.trueExpression = trueExpression;
            this.falseExpression = falseExpression;

            ternary = new SyntaxToken("?");
            alternate = new SyntaxToken(":");
        }

        internal TernaryExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(tree, parent, expression)
        {
            // Condition
            this.condition = Any(tree, this, expression.expression(0));

            // True expression
            this.trueExpression = Any(tree, this, expression.expression(1));

            // False expression
            this.falseExpression = Any(tree, this, expression.expression(2));

            ternary = new SyntaxToken(expression.ternary);
            alternate = new SyntaxToken(expression.alternate);
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
            alternate.GetSourceText(writer);

            // Write false
            falseExpression.GetSourceText(writer);
        }
    }
}
