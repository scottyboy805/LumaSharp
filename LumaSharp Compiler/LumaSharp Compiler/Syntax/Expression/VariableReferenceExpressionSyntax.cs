
namespace LumaSharp_Compiler.AST
{
    public sealed class VariableReferenceExpressionSyntax : ExpressionSyntax
    {
        // Private
        private SyntaxToken identifier = null;

        // Properties
        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal VariableReferenceExpressionSyntax(string identifier)
            : base(new SyntaxToken(identifier))
        {
            this.identifier = base.StartToken;
        }

        internal VariableReferenceExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(tree, parent, expression)
        {
            this.identifier = new SyntaxToken(expression.IDENTIFIER());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write identifier
            writer.Write(identifier.ToString());
        }
    }
}
