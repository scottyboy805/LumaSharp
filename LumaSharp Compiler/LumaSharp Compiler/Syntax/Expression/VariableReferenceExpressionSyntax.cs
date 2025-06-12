
using LumaSharp.Compiler.AST.Visitor;

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
        internal VariableReferenceExpressionSyntax(SyntaxToken identifier)
        {
            // Check kind
            if(identifier.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(identifier) + " must be of kind: " + SyntaxTokenKind.Identifier);

            this.identifier = identifier;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitVariableReferenceExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitVariableReferenceExpression(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write identifier
            writer.Write(identifier.ToString());
        }
    }
}
