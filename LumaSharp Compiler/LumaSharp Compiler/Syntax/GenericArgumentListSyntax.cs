
namespace LumaSharp.Compiler.AST
{
    public class GenericArgumentListSyntax : SeparatedListSyntax<TypeReferenceSyntax>
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
        internal GenericArgumentListSyntax(SyntaxNode parent, TypeReferenceSyntax[] genericArguments)
            : base(parent, SyntaxTokenKind.CommaSymbol)
        {
            this.lGeneric = Syntax.KeywordOrSymbol(SyntaxTokenKind.LGenericSymbol);
            this.rGeneric = Syntax.KeywordOrSymbol(SyntaxTokenKind.RGenericSymbol);

            foreach (TypeReferenceSyntax genericArgument in genericArguments)
                AddElement(genericArgument, Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol));
        }

        internal GenericArgumentListSyntax(SyntaxNode parent, LumaSharpParser.GenericArgumentListContext generics)
            : base(parent, SyntaxTokenKind.CommaSymbol)
        {
            lGeneric = new SyntaxToken(SyntaxTokenKind.LGenericSymbol, generics.LGENERIC());
            rGeneric = new SyntaxToken(SyntaxTokenKind.RGenericSymbol, generics.RGENERIC());

            // Get type reference list
            LumaSharpParser.TypeReferenceListContext typeList = generics.typeReferenceList();

            if (typeList != null)
            {
                // Add primary element
                LumaSharpParser.TypeReferenceContext primaryTypeArgument = typeList.typeReference();

                // Check for primary
                if (primaryTypeArgument != null)
                    AddElement(new TypeReferenceSyntax(this, null, primaryTypeArgument), null);


                // Add secondary elements
                LumaSharpParser.TypeReferenceSecondaryContext[] secondaryTypeArguments = typeList.typeReferenceSecondary();

                // Check for secondary
                if(secondaryTypeArguments != null)
                {
                    // Process all additional type arguments
                    foreach(LumaSharpParser.TypeReferenceSecondaryContext secondaryTypeArgument in secondaryTypeArguments)
                    {
                        AddElement(
                            new TypeReferenceSyntax(this, null, secondaryTypeArgument.typeReference()),
                            new SyntaxToken(SyntaxTokenKind.CommaSymbol, secondaryTypeArgument.COMMA()));
                    }
                }
            }
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
