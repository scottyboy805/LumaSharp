
namespace LumaSharp.Compiler.AST
{
    public class GenericArgumentListSyntax : SeparatedListSyntax<TypeReferenceSyntax>
    {
        // Private
        private readonly SyntaxToken lGeneric;
        private readonly SyntaxToken rGeneric;

        // Properties
        public override SyntaxToken StartToken => lGeneric;
        public override SyntaxToken EndToken => rGeneric;
        public SyntaxToken LGeneric => lGeneric;
        public SyntaxToken RGeneric => rGeneric;

        public bool HasGenericArguments
        {
            get { return Count > 0; }
        }

        // Constructor
        internal GenericArgumentListSyntax(SeparatedListSyntax<TypeReferenceSyntax> genericArguments)
            : this(
                  new SyntaxToken(SyntaxTokenKind.LessSymbol),
                  genericArguments,
                  new SyntaxToken(SyntaxTokenKind.GreaterSymbol))
        {
        }

        internal GenericArgumentListSyntax(SyntaxToken lGeneric, SeparatedListSyntax<TypeReferenceSyntax> genericArguments, SyntaxToken rGeneric)
            : base(genericArguments)
        {
            // Check kind
            if(lGeneric.Kind != SyntaxTokenKind.LessSymbol)
                throw new ArgumentException(nameof(lGeneric) + " must be of kind: " + SyntaxTokenKind.LessSymbol);

            if(rGeneric.Kind != SyntaxTokenKind.GreaterSymbol)
                throw new ArgumentException(nameof(rGeneric) + " must be of kind: " + SyntaxTokenKind.GreaterSymbol);

            this.lGeneric = lGeneric;
            this.rGeneric = rGeneric;
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
