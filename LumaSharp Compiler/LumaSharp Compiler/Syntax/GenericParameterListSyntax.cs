
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
        internal GenericParameterListSyntax(SyntaxNode parent, GenericParameterSyntax[] genericParameters)
            : base(parent, SyntaxTokenKind.CommaSymbol)
        {
            this.lGeneric = Syntax.KeywordOrSymbol(SyntaxTokenKind.LGenericSymbol);
            this.rGeneric = Syntax.KeywordOrSymbol(SyntaxTokenKind.RGenericSymbol);

            foreach (GenericParameterSyntax genericParameter in genericParameters)
                AddElement(genericParameter, Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol));
        }

        internal GenericParameterListSyntax(SyntaxNode parent, LumaSharpParser.GenericParameterListContext generics)
            : base(parent, SyntaxTokenKind.CommaSymbol)
        {
            this.lGeneric = new SyntaxToken(SyntaxTokenKind.LGenericSymbol, generics.LGENERIC());
            this.rGeneric = new SyntaxToken(SyntaxTokenKind.RGenericSymbol, generics.RGENERIC());

            // Add primary element
            LumaSharpParser.GenericParameterContext primaryGenericParameter = generics.genericParameter();

            // Check for primary
            if (primaryGenericParameter != null)
                AddElement(new GenericParameterSyntax(this, primaryGenericParameter, 0), null);


            // Add secondary elements
            LumaSharpParser.GenericParameterSecondaryContext[] secondaryGenericParameters = generics.genericParameterSecondary();

            // Check for secondary
            if(secondaryGenericParameters != null)
            {
                int index = 1;

                // Process all additional parameters
                foreach (LumaSharpParser.GenericParameterSecondaryContext secondaryGenericParameter in secondaryGenericParameters)
                {
                    AddElement(
                        new GenericParameterSyntax(this, secondaryGenericParameter.genericParameter(), index++),
                        new SyntaxToken(SyntaxTokenKind.CommaSymbol, secondaryGenericParameter.COMMA()));
                }
            }
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
