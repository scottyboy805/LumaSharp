
namespace LumaSharp_Compiler.AST
{
    public sealed class BaseExpressionSyntax : ExpressionSyntax
    {
        // Private
        private SyntaxToken keyword = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal BaseExpressionSyntax()
            : base(SyntaxToken.Base())
        {
            this.keyword = base.StartToken;
        }

        internal BaseExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(tree, parent, expression)
        {
            keyword = new SyntaxToken(expression.BASE());
        }


        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write source
            keyword.GetSourceText(writer);
        }
    }
}
