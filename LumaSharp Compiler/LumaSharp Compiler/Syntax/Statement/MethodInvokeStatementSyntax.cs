

using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class MethodInvokeStatementSyntax : StatementSyntax
    {
        // Private
        private readonly MethodInvokeExpressionSyntax invokeExpression;
        private readonly SyntaxToken semicolon;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return invokeExpression.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return semicolon; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        public MethodInvokeExpressionSyntax InvokeExpression
        {
            get { return invokeExpression; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return invokeExpression; }
        }

        // Constructor
        internal MethodInvokeStatementSyntax(MethodInvokeExpressionSyntax invokeExpression)
            : this(
                  invokeExpression,
                  Syntax.Token(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal MethodInvokeStatementSyntax(MethodInvokeExpressionSyntax invokeExpression, SyntaxToken semicolon)
        {
            // Check for null
            if(invokeExpression == null)
                throw new ArgumentNullException(nameof(invokeExpression));

            // Check kind
            if(semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);

            this.invokeExpression = invokeExpression;
            this.semicolon = semicolon;

            // Set parent
            invokeExpression.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitMethodInvokeStatement(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Invoke
            invokeExpression.GetSourceText(writer);

            // Semicolon
            semicolon.GetSourceText(writer);
        }
    }
}
