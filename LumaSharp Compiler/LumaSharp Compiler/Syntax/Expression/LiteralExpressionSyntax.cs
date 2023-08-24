
namespace LumaSharp_Compiler.Syntax
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        // Private
        private SyntaxToken value = null;

        // Properties
        public SyntaxToken Value
        {
            get { return value; }
        }

        public override SyntaxToken StartToken
        {
            get { return value; }
        }

        public override SyntaxToken EndToken
        {
            get { return value; }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write value
            writer.Write(value.ToString());
        }
    }
}
