
namespace LumaSharp.Compiler.AST
{
    public sealed class GenericParameterListSyntax : SeparatedSyntaxList<GenericParameterSyntax>
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

        public bool HasGenericParameters
        {
            get { return Count > 0; }
        }

        // Constructor
        internal GenericParameterListSyntax(SeparatedSyntaxList<GenericParameterSyntax> genericParameters)
            : this(
                  new SyntaxToken(SyntaxTokenKind.LessSymbol),
                  genericParameters,
                  new SyntaxToken(SyntaxTokenKind.GreaterSymbol))
        {
        }

        internal GenericParameterListSyntax(SyntaxToken lGeneric, SeparatedSyntaxList<GenericParameterSyntax> genericParameters, SyntaxToken rGeneric)
            : base(genericParameters)
        {
            // Check kind
            if (lGeneric.Kind != SyntaxTokenKind.LessSymbol)
                throw new ArgumentException(nameof(lGeneric) + " must be of kind: " + SyntaxTokenKind.LessSymbol);

            if (rGeneric.Kind != SyntaxTokenKind.GreaterSymbol)
                throw new ArgumentException(nameof(rGeneric) + " must be of kind: " + SyntaxTokenKind.GreaterSymbol);

            this.lGeneric = lGeneric;
            this.rGeneric = rGeneric;

            // Set parent
            if(genericParameters != null) genericParameters.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Generic start
            lGeneric.GetSourceText(writer);

            // Generic parameters
            base.GetSourceText(writer);

            // Generic end
            rGeneric.GetSourceText(writer);
        }
    }
}
