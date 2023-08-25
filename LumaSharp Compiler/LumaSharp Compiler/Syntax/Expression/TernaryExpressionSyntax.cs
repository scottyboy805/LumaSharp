
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

        public override SyntaxToken StartToken
        {
            get { return condition.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return falseExpression.EndToken; }
        }

        // Constructor
        internal TernaryExpressionSyntax(SyntaxTree tree, SyntaxNode parent)
            : base(tree, parent)
        {
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
