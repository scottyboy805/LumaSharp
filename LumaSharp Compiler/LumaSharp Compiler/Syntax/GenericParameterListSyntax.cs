
namespace LumaSharp.Compiler.AST
{
    public sealed class GenericParameterListSyntax : SeparatedListSyntax<GenericParameterSyntax>
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
        internal GenericParameterListSyntax(SeparatedListSyntax<GenericParameterSyntax> genericParameters)
            : this(
                  new SyntaxToken(SyntaxTokenKind.LessSymbol),
                  genericParameters,
                  new SyntaxToken(SyntaxTokenKind.GreaterSymbol))
        {
        }

        internal GenericParameterListSyntax(SyntaxToken lGeneric, SeparatedListSyntax<GenericParameterSyntax> genericParameters, SyntaxToken rGeneric)
            : base(genericParameters)
        {
            this.lGeneric = lGeneric;
            this.rGeneric = rGeneric;
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
