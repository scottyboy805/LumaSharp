
using LumaSharp_Compiler.AST.Expression;

namespace LumaSharp_Compiler.AST.Statement
{
    public sealed class MethodInvokeStatementSyntax : StatementSyntax
    {
        // Private
        private MethodInvokeExpressionSyntax invokeExpression = null;

        // Properties
        public MethodInvokeExpressionSyntax InvokeExpression
        {
            get { return invokeExpression; }
        }

        // Constructor
        internal MethodInvokeStatementSyntax(MethodInvokeExpressionSyntax invokeExpression)
            : base(invokeExpression.StartToken)
        {
            this.invokeExpression = invokeExpression;
        }

        internal MethodInvokeStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.MethodInvokeStatementContext invoke)
            : base(tree, parent, invoke)
        {
            // Get invoke
            this.invokeExpression = new MethodInvokeExpressionSyntax(tree, this, invoke.expression(), invoke.methodInvokeExpression());

            // Semicolon
            this.statementEnd = new SyntaxToken(invoke.semi);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Invoke
            invokeExpression.GetSourceText(writer);

            // Semicolon
            this.statementEnd.GetSourceText(writer);
        }
    }
}
