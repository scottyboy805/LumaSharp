
namespace LumaSharp.Compiler.AST
{
    public sealed class TypeExpressionSyntax : ExpressionSyntax
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
        internal TypeExpressionSyntax(SyntaxNode parent, TypeReferenceSyntax typeReference)
            : base(parent, null)
        {
            this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.TypeKeyword);
            this.lParen = Syntax.KeywordOrSymbol(SyntaxTokenKind.LParenSymbol);
            this.rParen = Syntax.KeywordOrSymbol(SyntaxTokenKind.RParenSymbol);

            // Type reference
            this.typeReference = typeReference;
        }

        internal TypeExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            LumaSharpParser.TypeExpressionContext type = expression.typeExpression();

            // Keyword
            this.keyword = new SyntaxToken(SyntaxTokenKind.TypeKeyword, type.TYPE());

            // LR paren
            this.lParen = new SyntaxToken(SyntaxTokenKind.LParenSymbol, type.LPAREN());
            this.rParen = new SyntaxToken(SyntaxTokenKind.RParenSymbol, type.RPAREN());

            // Type reference
            this.typeReference = new TypeReferenceSyntax(this, null, type.typeReference());
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
