
namespace LumaSharp.Compiler.AST
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken value;
        private readonly SyntaxToken descriptor;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return value; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for descriptor
                if (HasDescriptor == true)
                    return descriptor;

                return value;
            }
        }

        public SyntaxToken Value
        {
            get { return value; }
        }

        public SyntaxToken Descriptor
        {
            get { return descriptor; }
        }

        public bool HasDescriptor
        {
            get { return descriptor.Kind != SyntaxTokenKind.Invalid; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal LiteralExpressionSyntax(SyntaxNode parent, SyntaxToken value, SyntaxToken? descriptor = null)
            : base(parent, null)
        {
            this.value = value;
            this.descriptor = descriptor != null ? descriptor.Value : default;
        }

        internal LiteralExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            LumaSharpParser.LiteralExpressionContext end = expression.literalExpression();

            // Create value
            this.value = new SyntaxToken(SyntaxTokenKind.Literal, end.Start);

            // Create descriptor
            if (end.Start != end.Stop)
                this.descriptor = new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, end.Stop);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write value
            writer.Write(value.Text);

            // Write descriptor
            if (HasDescriptor == true)
                writer.Write(descriptor.Text);
        }
    }
}
