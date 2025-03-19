
namespace LumaSharp.Compiler.AST
{
    public sealed class SizeExpressionSyntax : ExpressionSyntax
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
        internal SizeExpressionSyntax(SyntaxNode parent, TypeReferenceSyntax typeReference)
            : base(parent, null)
        {
            this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.SizeKeyword);
            this.lParen = Syntax.KeywordOrSymbol(SyntaxTokenKind.LParenSymbol);
            this.rParen = Syntax.KeywordOrSymbol(SyntaxTokenKind.RParenSymbol);

            // Type reference
            this.typeReference = typeReference;
        }

        internal SizeExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            LumaSharpParser.SizeExpressionContext size = expression.sizeExpression();

            // Keyword
            this.keyword = new SyntaxToken(SyntaxTokenKind.SizeKeyword, size.SIZE());

            // LR paren
            this.lParen = new SyntaxToken(SyntaxTokenKind.LParenSymbol, size.LPAREN());
            this.rParen = new SyntaxToken(SyntaxTokenKind.RParenSymbol, size.RPAREN());

            // Type reference
            this.typeReference = new TypeReferenceSyntax(this, null, size.typeReference());
        }

        // Methods
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
