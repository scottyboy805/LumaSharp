
namespace LumaSharp_Compiler.Syntax
{
    public sealed class TypeExpressionSyntax : ExpressionSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private SyntaxToken start = null;
        private SyntaxToken end = null;
        private TypeReferenceSyntax typeReference = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return end; }
        }

        public TypeReferenceSyntax TypeReference
        {
            get { return typeReference; }
        }

        // Constructor
        internal TypeExpressionSyntax(TypeReferenceSyntax typeReference, SyntaxTree tree, SyntaxNode node)
            : base(tree, node)
        {
            this.keyword = new SyntaxToken("type");
            this.start = new SyntaxToken("(");
            this.end = new SyntaxToken(")");
            this.typeReference = typeReference;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            writer.Write(keyword.ToString());

            // Write opening
            writer.Write(start.ToString());

            // Write type
            typeReference.GetSourceText(writer);

            // Write closing
            writer.Write(end.ToString());
        }
    }
}
