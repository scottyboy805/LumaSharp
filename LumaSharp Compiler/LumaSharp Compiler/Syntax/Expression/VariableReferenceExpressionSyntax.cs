
namespace LumaSharp.Compiler.AST
{
    public sealed class VariableReferenceExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken identifier;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return identifier; }
        }

        public override SyntaxToken EndToken
        {
            get { return identifier; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal VariableReferenceExpressionSyntax(SyntaxNode parent, string identifier)
            : base(parent, null)
        {
            this.identifier = Syntax.Identifier(identifier);
        }

        internal VariableReferenceExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            this.identifier = new SyntaxToken(SyntaxTokenKind.Identifier, expression.IDENTIFIER());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write identifier
            writer.Write(identifier.ToString());
        }
    }
}
