
namespace LumaSharp.Compiler.AST
{
    public sealed class TypeofExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly SyntaxToken lParen;
        private readonly SyntaxToken rParen;
        private readonly TypeReferenceSyntax typeReference;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return rParen; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public new SyntaxToken LParen
        {
            get { return lParen; }
        }

        public new SyntaxToken RParen
        {
            get { return rParen; }
        }

        public TypeReferenceSyntax TypeReference
        {
            get { return typeReference; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return typeReference; }
        }

        // Constructor
        internal TypeofExpressionSyntax(TypeReferenceSyntax typeReference)
            : this(
                  new SyntaxToken(SyntaxTokenKind.TypeofKeyword),
                  new SyntaxToken(SyntaxTokenKind.LParenSymbol),
                  typeReference,
                  new SyntaxToken(SyntaxTokenKind.RParenSymbol))
        {
        }

        internal TypeofExpressionSyntax(SyntaxToken keyword, SyntaxToken lParen, TypeReferenceSyntax typeReference, SyntaxToken rParen)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.TypeofKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.TypeofKeyword);

            if(lParen.Kind != SyntaxTokenKind.LParenSymbol)
                throw new ArgumentException(nameof(lParen) + " must be of kind: " + SyntaxTokenKind.LParenSymbol);

            if(rParen.Kind != SyntaxTokenKind.RParenSymbol)
                throw new ArgumentException(nameof(rParen) + " must be of kind: " + SyntaxTokenKind.RParenSymbol);

            // Check for null
            if(typeReference == null)
                throw new ArgumentNullException(nameof(typeReference));

            this.keyword = keyword;
            this.lParen = lParen;
            this.rParen = rParen;

            // Type reference
            this.typeReference = typeReference;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            keyword.GetSourceText(writer);

            // LParen
            lParen.GetSourceText(writer);

            // Expression
            typeReference.GetSourceText(writer);

            // RParen
            rParen.GetSourceText(writer);
        }
    }
}
