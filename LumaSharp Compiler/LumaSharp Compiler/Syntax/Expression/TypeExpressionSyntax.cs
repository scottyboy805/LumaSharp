
namespace LumaSharp_Compiler.Syntax
{
    public sealed class TypeExpressionSyntax : ExpressionSyntax
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
        internal TypeExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.TypeExpressionContext type)
            : base(tree, parent, type)
        {
            // Keyword
            this.keyword = new SyntaxToken(type.TYPE());

            // LR paren
            this.lparen = new SyntaxToken(type.lparen);
            this.rparen = new SyntaxToken(type.rparen);

            // Type reference
            this.typeReference = new TypeReferenceSyntax(tree, this, type.typeReference());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            keyword.GetSourceText(writer);

            // Lparen
            lparen.GetSourceText(writer);

            // Expression
            typeReference.GetSourceText(writer);

            // Rparen
            rparen.GetSourceText(writer);
        }
    }
}
