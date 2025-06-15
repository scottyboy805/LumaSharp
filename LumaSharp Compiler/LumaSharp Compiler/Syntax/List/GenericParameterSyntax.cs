using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class GenericParameterSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken identifier;
        private readonly GenericParameterConstraintsSyntax constraints;      

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
                if (HasConstraints == true)
                    return constraints.EndToken;

                return identifier;
            }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public GenericParameterConstraintsSyntax Constraints
        {
            get { return constraints; }
        }

        public int ConstraintCount
        {
            get { return HasConstraints ? constraints.Constraints.Count : 0; }
        }

        public bool HasConstraints
        {
            get { return constraints != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                if (constraints != null)
                    yield return constraints;
            }
        }

        // Constructor
        private GenericParameterSyntax()
        {
            this.identifier = Syntax.Identifier("Error");
        }

        internal GenericParameterSyntax(SyntaxToken identifier, GenericParameterConstraintsSyntax constraints)
        {
            // Check kind
            if (identifier.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(identifier) + " must be of kind: " + SyntaxTokenKind.Identifier);

            this.identifier = identifier;
            this.constraints = constraints;

            // Set parent
            if (constraints != null) constraints.parent = this;
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

            if (HasConstraints == true)
            {
                // Get constrains types
                constraints.GetSourceText(writer);
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
