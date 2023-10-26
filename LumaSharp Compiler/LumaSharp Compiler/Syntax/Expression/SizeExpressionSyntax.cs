
namespace LumaSharp_Compiler.AST
{
    public sealed class SizeExpressionSyntax : ExpressionSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private SyntaxToken lparen = null;
        private SyntaxToken rparen = null;
        private TypeReferenceSyntax typeReference = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
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
        internal SizeExpressionSyntax(TypeReferenceSyntax typeReference)
            : base(SyntaxToken.Size(), SyntaxToken.RParen())
        {
            this.keyword = base.StartToken;
            this.lparen = SyntaxToken.LParen();
            this.rparen = base.EndToken;

            // Type reference
            this.typeReference = typeReference;
        }

        internal SizeExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.SizeExpressionContext size)
            : base(tree, parent, size)
        {
            // Keyword
            this.keyword = new SyntaxToken(size.SIZE());

            // LR paren
            this.lparen = new SyntaxToken(size.lparen);
            this.rparen = new SyntaxToken(size.rparen);

            // Type reference
            this.typeReference = new TypeReferenceSyntax(tree, this, size.typeReference());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            writer.Write(keyword.ToString());

            // Write opening
            writer.Write(lparen.ToString());

            // Write type
            typeReference.GetSourceText(writer);

            // Write closing
            writer.Write(rparen.ToString());
        }
    }
}
