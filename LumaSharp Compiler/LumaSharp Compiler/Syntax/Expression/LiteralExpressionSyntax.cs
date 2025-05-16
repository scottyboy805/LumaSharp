
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
        internal LiteralExpressionSyntax(SyntaxToken value, SyntaxToken? descriptor = null)
        {
            // Check kind
            if(value.Kind != SyntaxTokenKind.Literal)
                throw new ArgumentException(nameof(value) + " must be of kind: " + SyntaxTokenKind.Literal);

            if (descriptor != null && descriptor.Value.Kind != SyntaxTokenKind.LiteralDescriptor)
                throw new ArgumentException(nameof(descriptor) + " must be of kind: " + SyntaxTokenKind.LiteralDescriptor);

            this.value = value;
            this.descriptor = descriptor != null ? descriptor.Value : default;
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
