
namespace LumaSharp_Compiler.Syntax
{
    public sealed class VariableReferenceExpression : ExpressionSyntax
    {
        // Private
        private SyntaxToken identifier = null;

        // Properties
        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public override SyntaxToken StartToken
        {
            get { return identifier; }
        }

        public override SyntaxToken EndToken
        {
            get { return identifier; }
        }

        // Constructor
        internal VariableReferenceExpression(SyntaxTree tree, SyntaxNode parent, string identifier)
            : base(tree, parent)
        {
            this.identifier = new SyntaxToken(identifier);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write identifier
            writer.Write(identifier.ToString());
        }
    }
}
