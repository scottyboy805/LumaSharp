
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
            : base(parent)
        {
            this.invokeExpression = invokeExpression;
        }

        internal MethodInvokeStatementSyntax(SyntaxNode parent, LumaSharpParser.MethodInvokeStatementContext invoke)
            : base(parent)
        {
            // Get invoke
            this.invokeExpression = new MethodInvokeExpressionSyntax(this, invoke.expression());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Invoke
            invokeExpression.GetSourceText(writer);
        }
    }
}
