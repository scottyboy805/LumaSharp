
namespace LumaSharp_Compiler.Syntax
{
    public sealed class FieldAccessorReferenceExpression : ExpressionSyntax
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

        public override SyntaxToken StartToken
        {
            get { return accessExpression.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return identifier; }
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
