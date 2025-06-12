
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class GenericParameterSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken identifier;
        private readonly SeparatedSyntaxList<TypeReferenceSyntax> genericConstraints;
        private readonly SyntaxToken colon;

        // Internal
        internal static readonly GenericParameterSyntax Error = new();

        // Properties
        public override SyntaxToken StartToken
        {
            get { return identifier; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for constraint
                if (HasConstraintTypes == true)
                    return genericConstraints.EndToken;

                return identifier;
            }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public SeparatedSyntaxList<TypeReferenceSyntax> ConstraintTypes
        {
            get { return genericConstraints; }
        }

        public int ConstraintTypeCount
        {
            get { return HasConstraintTypes ? genericConstraints.Count : 0; }
        }

        public bool HasConstraintTypes
        {
            get { return genericConstraints != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                return genericConstraints != null
                    ? genericConstraints.Descendants
                    : Enumerable.Empty<SyntaxNode>();
            }
        }

        // Constructor
        private GenericParameterSyntax()
        {
            this.identifier = Syntax.Identifier("Error");
        }

        internal GenericParameterSyntax(SyntaxToken identifier, SyntaxToken colon, SeparatedSyntaxList<TypeReferenceSyntax> constraintTypes)
        {
            // Check kind
            if (identifier.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(identifier) + " must be of kind: " + SyntaxTokenKind.Identifier);

            if (colon.Kind != SyntaxTokenKind.Invalid && colon.Kind != SyntaxTokenKind.ColonSymbol)
                throw new ArgumentException(nameof(colon) + " must be of kind: " + SyntaxTokenKind.ColonSymbol);

            this.identifier = identifier;
            this.colon = colon;
            this.genericConstraints = constraintTypes;

            // Set parent
            if (constraintTypes != null) constraintTypes.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitGenericParameter(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitGenericParameter(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write identifier
            identifier.GetSourceText(writer);

            if (HasConstraintTypes == true)
            {
                // Colon
                colon.GetSourceText(writer);

                // Get constrains types
                genericConstraints.GetSourceText(writer);
            }
        }

        public int GetPositionIndex()
        {
            // Try to find index
            if (parent is GenericParameterListSyntax genericParameterList)
                return genericParameterList.IndexOf(this);

            // Invalid index
            return -1;
        }
    }
}
