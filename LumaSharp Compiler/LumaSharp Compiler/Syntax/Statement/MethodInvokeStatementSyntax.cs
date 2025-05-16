
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
        internal MethodInvokeStatementSyntax(SyntaxNode parent, MethodInvokeExpressionSyntax invokeExpression)
        {
            this.invokeExpression = invokeExpression;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Invoke
            invokeExpression.GetSourceText(writer);
        }
    }
}
