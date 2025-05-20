
namespace LumaSharp.Compiler.AST
{
    public sealed class MethodInvokeStatementSyntax : StatementSyntax
    {
        // Private
        private readonly MethodInvokeExpressionSyntax invokeExpression;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return invokeExpression.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return invokeExpression.EndToken; }
        }

        public MethodInvokeExpressionSyntax InvokeExpression
        {
            get { return invokeExpression; }
        }

        // Constructor
        internal MethodInvokeStatementSyntax(MethodInvokeExpressionSyntax invokeExpression)
        {
            // Check for null
            if(invokeExpression == null)
                throw new ArgumentNullException(nameof(invokeExpression));

            this.invokeExpression = invokeExpression;

            // Set parent
            invokeExpression.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Invoke
            invokeExpression.GetSourceText(writer);
        }
    }
}
