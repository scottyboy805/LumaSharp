
namespace LumaSharp_Compiler.Syntax
{
    public sealed class FieldAccessorReferenceExpressionSyntax : ExpressionSyntax
    {
        // Private
        private ExpressionSyntax accessExpression = null;
        private SyntaxToken identifier = null;

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
        internal FieldAccessorReferenceExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(tree, parent, expression)
        {
            // Identifier
            this.identifier = new SyntaxToken(expression.fieldAccessExpression().IDENTIFIER());

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
            writer.Write('.');

            // Write identifier
            writer.Write(identifier.ToString());
        }
    }
}
