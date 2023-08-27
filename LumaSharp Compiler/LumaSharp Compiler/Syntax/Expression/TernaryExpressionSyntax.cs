
namespace LumaSharp_Compiler.Syntax
{
    public sealed class TernaryExpressionSyntax : ExpressionSyntax
    {
        // Private
        private ExpressionSyntax condition = null;
        private ExpressionSyntax trueExpression = null;
        private ExpressionSyntax falseExpression = null;

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
        internal TernaryExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(tree, parent, expression)
        {
            // Condition
            this.condition = Any(tree, this, expression.expression(0));

            // True expression
            this.trueExpression = Any(tree, this, expression.expression(1));

            // False expression
            this.falseExpression = Any(tree, this, expression.expression(2));
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
