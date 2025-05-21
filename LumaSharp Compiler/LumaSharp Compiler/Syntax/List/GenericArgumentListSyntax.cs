
namespace LumaSharp.Compiler.AST
{
    public class GenericArgumentListSyntax : SeparatedSyntaxList<TypeReferenceSyntax>
    {
        // Private
        private readonly SyntaxToken lGeneric;
        private readonly SyntaxToken rGeneric;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lGeneric; }
        }

        public override SyntaxToken EndToken
        {
            get { return rGeneric; }
        }

        public SyntaxToken LGeneric
        {
            get { return lGeneric; }
        }

        public SyntaxToken RGeneric
        {
            get { return rGeneric; }
        }

        public bool HasGenericArguments
        {
            get { return Count > 0; }
        }

        // Constructor
        internal GenericArgumentListSyntax(SeparatedSyntaxList<TypeReferenceSyntax> genericArguments)
            : this(
                  new SyntaxToken(SyntaxTokenKind.LessSymbol),
                  genericArguments,
                  new SyntaxToken(SyntaxTokenKind.GreaterSymbol))
        {
        }

        internal GenericArgumentListSyntax(SyntaxToken lGeneric, SeparatedSyntaxList<TypeReferenceSyntax> genericArguments, SyntaxToken rGeneric)
            : base(genericArguments)
        {
            // Check kind
            if(lGeneric.Kind != SyntaxTokenKind.LessSymbol)
                throw new ArgumentException(nameof(lGeneric) + " must be of kind: " + SyntaxTokenKind.LessSymbol);

            if(rGeneric.Kind != SyntaxTokenKind.GreaterSymbol)
                throw new ArgumentException(nameof(rGeneric) + " must be of kind: " + SyntaxTokenKind.GreaterSymbol);

            this.lGeneric = lGeneric;
            this.rGeneric = rGeneric;

            // Set parent
            if (genericArguments != null) genericArguments.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Generic start
            lGeneric.GetSourceText(writer);

            // Generic arguments
            base.GetSourceText(writer);

            // Generic end
            rGeneric.GetSourceText(writer);
        }
    }
}
