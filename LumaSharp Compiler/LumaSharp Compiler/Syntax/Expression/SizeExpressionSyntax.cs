
namespace LumaSharp_Compiler.Syntax
{
    public sealed class SizeExpressionSyntax : ExpressionSyntax
    {
        // Private
        private SyntaxToken keyword = null;
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
        internal SizeExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.SizeExpressionContext size)
            : base(tree, parent, size)
        {
            // Keyword
            this.keyword = new SyntaxToken(size.SIZE());

            // Type reference
            this.typeReference = new TypeReferenceSyntax(tree, this, size.typeReference());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            writer.Write(keyword.ToString());

            // Write opening
            //writer.Write(start.ToString());

            // Write type
            typeReference.GetSourceText(writer);

            // Write closing
            //writer.Write(end.ToString());
        }
    }
}
