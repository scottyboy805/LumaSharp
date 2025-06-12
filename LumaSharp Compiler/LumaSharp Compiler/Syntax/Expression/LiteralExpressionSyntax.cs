
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken value;
        private readonly SyntaxToken? descriptor;

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
                if(descriptor != null)
                    return descriptor.Value;

                return value;
            }
        }

        public SyntaxToken Value
        {
            get { return value; }
        }

        public SyntaxToken? Descriptor
        {
            get { return descriptor; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal LiteralExpressionSyntax(SyntaxToken value, SyntaxToken? descriptor = null)
        {
            // Check kind
            if(value.IsLiteral == false)
                throw new ArgumentException(nameof(value) + " must be a valid literal");

            if (descriptor != null && descriptor.Value.Kind != SyntaxTokenKind.LiteralDescriptor)
                throw new ArgumentException(nameof(descriptor) + " must be of kind: " + SyntaxTokenKind.LiteralDescriptor);

            this.value = value;
            this.descriptor = descriptor;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitLiteralExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write value
            value.GetSourceText(writer);

            // Write descriptor
            if (descriptor != null)
                descriptor?.GetSourceText(writer);
        }
    }
}
