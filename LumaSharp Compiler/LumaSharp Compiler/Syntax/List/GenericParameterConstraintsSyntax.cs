using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class GenericParameterConstraintsSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken colon;
        private readonly SeparatedSyntaxList<TypeReferenceSyntax> constraints;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return colon; }
        }

        public override SyntaxToken EndToken
        {
            get { return constraints.EndToken; }
        }

        public SyntaxToken Colon
        {
            get { return colon; }
        }

        public SeparatedSyntaxList<TypeReferenceSyntax> Constraints
        {
            get { return constraints; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return constraints; }
        }

        // Constructor
        internal GenericParameterConstraintsSyntax(SyntaxToken colon, SeparatedSyntaxList<TypeReferenceSyntax> constraints)
        {
            // Check kind
            if(colon.Kind != SyntaxTokenKind.ColonSymbol)
                throw new ArgumentException(nameof(colon) + " must be of kind: " + SyntaxTokenKind.ColonSymbol);

            // Check for null
            if(constraints == null)
                throw new ArgumentNullException(nameof(constraints));

            this.colon = colon;
            this.constraints = constraints;

            // Set parent
            constraints.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitGenericParameterConstraints(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitGenericParameterConstraints(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Colon
            colon.GetSourceText(writer);

            // Get constraints
            constraints.GetSourceText(writer);
        }
    }
}
