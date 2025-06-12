
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class SizeofExpressionSyntax : ExpressionSyntax
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

        public SyntaxToken LParen
        {
            get { return lParen; }
        }

        public SyntaxToken RParen
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
        internal SizeofExpressionSyntax(TypeReferenceSyntax typeReference)
            : this(
                  Syntax.Token(SyntaxTokenKind.SizeofKeyword),
                  Syntax.Token(SyntaxTokenKind.LParenSymbol),
                  typeReference,
                  Syntax.Token(SyntaxTokenKind.RParenSymbol))
        {
        }

        internal SizeofExpressionSyntax(SyntaxToken keyword, SyntaxToken lParen, TypeReferenceSyntax typeReference, SyntaxToken rParen)
        {
            // Check kind
            if (keyword.Kind != SyntaxTokenKind.SizeofKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.SizeofKeyword);

            if (lParen.Kind != SyntaxTokenKind.LParenSymbol)
                throw new ArgumentException(nameof(lParen) + " must be of kind: " + SyntaxTokenKind.LParenSymbol);

            if (rParen.Kind != SyntaxTokenKind.RParenSymbol)
                throw new ArgumentException(nameof(rParen) + " must be of kind: " + SyntaxTokenKind.RParenSymbol);

            // Check for null
            if (typeReference == null)
                throw new ArgumentNullException(nameof(typeReference));

            this.keyword = keyword;
            this.lParen = lParen;
            this.rParen = rParen;

            // Type reference
            this.typeReference = typeReference;

            // Set parent
            typeReference.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitSizeofExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitSizeofExpression(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            keyword.GetSourceText(writer);

            // Write opening
            lParen.GetSourceText(writer);

            // Write type
            typeReference.GetSourceText(writer);

            // Write closing
            rParen.GetSourceText(writer);
        }
    }
}
