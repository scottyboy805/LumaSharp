
namespace LumaSharp_Compiler.AST
{
    public sealed class FieldAccessorReferenceExpressionSyntax : ExpressionSyntax
    {
        // Private
        private ExpressionSyntax accessExpression = null;
        private SyntaxToken identifier = null;
        private SyntaxToken dot = null;

        // Properties
        public ExpressionSyntax AccessExpression
        {
            get { return accessExpression; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return accessExpression; }
        }

        // Constructor
        internal FieldAccessorReferenceExpressionSyntax(string identifier, ExpressionSyntax accessExpression)
            : base(new SyntaxToken(identifier))
        {
            this.identifier = base.StartToken;
            this.accessExpression = accessExpression;
            this.dot = new SyntaxToken(".");
        }

        internal FieldAccessorReferenceExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(tree, parent, expression)
        {
            // Identifier
            this.identifier = new SyntaxToken(expression.fieldAccessExpression().IDENTIFIER());

            // Dot
            this.dot = new SyntaxToken(expression.fieldAccessExpression().dot);

            // Create access expression
            if (expression.typeReference() != null)
            {
                this.accessExpression = new TypeReferenceSyntax(tree, this, expression.typeReference());
            }
            else
            {
                this.accessExpression = Any(tree, this, expression.expression(0));
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write access expression
            accessExpression.GetSourceText(writer);

            // Write dot
            dot.GetSourceText(writer);

            // Write identifier
            writer.Write(identifier.ToString());
        }
    }
}
