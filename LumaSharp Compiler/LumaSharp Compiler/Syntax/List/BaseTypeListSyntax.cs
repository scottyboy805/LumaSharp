
namespace LumaSharp.Compiler.AST
{
    public sealed class BaseTypeListSyntax : SeparatedSyntaxList<TypeReferenceSyntax>
    {
        // Private
        private readonly SyntaxToken colon;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return colon; }
        }

        public SyntaxToken Colon
        {
            get { return colon; }
        }

        // Constructor
        internal BaseTypeListSyntax(SeparatedSyntaxList<TypeReferenceSyntax> baseTypes)
            : this(
                  Syntax.Token(SyntaxTokenKind.ColonSymbol),
                  baseTypes)
        {
        }

        internal BaseTypeListSyntax(SyntaxToken colon, SeparatedSyntaxList<TypeReferenceSyntax> baseTypes)
            : base(baseTypes)
        {
            // Check kind
            if(colon.Kind != SyntaxTokenKind.ColonSymbol)
                throw new ArgumentException(nameof(colon) + " must be of kind: " + SyntaxTokenKind.ColonSymbol);

            this.colon = colon;

            // Set parent
            if (baseTypes != null) baseTypes.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write colon
            colon.GetSourceText(writer);
            
            // Write separated list
            base.GetSourceText(writer);
        }
    }
}
