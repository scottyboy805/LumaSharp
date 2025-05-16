
namespace LumaSharp.Compiler.AST
{
    public sealed class GenericParameterSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken identifier;
        private readonly SeparatedListSyntax<TypeReferenceSyntax> genericConstraints;
        private readonly SyntaxToken colon;
        private readonly int index;

        // Internal
        internal static readonly GenericParameterSyntax Error = new();

        // Properties
        public override SyntaxToken StartToken => identifier;
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

        public SyntaxToken Identifier => identifier;
        public SeparatedListSyntax<TypeReferenceSyntax> ConstraintTypes => genericConstraints;
        public int Index => index;
        public int ConstraintTypeCount => HasConstraintTypes ? genericConstraints.Count : 0;
        public bool HasConstraintTypes => genericConstraints != null;
        internal override IEnumerable<SyntaxNode> Descendants => genericConstraints != null 
            ? genericConstraints.Descendants 
            : Enumerable.Empty<SyntaxNode>();

        // Constructor
        private GenericParameterSyntax()
        {
            this.identifier = new SyntaxToken(SyntaxTokenKind.Identifier, "Error");
        }

        internal GenericParameterSyntax(SyntaxToken identifier, int index, SyntaxToken colon, SeparatedListSyntax<TypeReferenceSyntax> contraintTypes)
        {
            // Check kind
            if (identifier.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(identifier) + " must be of kind: " + SyntaxTokenKind.Identifier);

            if (colon.Kind != SyntaxTokenKind.Invalid && colon.Kind != SyntaxTokenKind.ColonSymbol)
                throw new ArgumentException(nameof(colon) + " must be of kind: " + SyntaxTokenKind.ColonSymbol);

            this.identifier = identifier;
            this.index = index;
            this.colon = colon;
            this.genericConstraints = contraintTypes;
        }

        // Methods
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
    }
}
