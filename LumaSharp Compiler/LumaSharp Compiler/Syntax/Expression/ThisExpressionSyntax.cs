
namespace LumaSharp_Compiler.AST
{
    public sealed class ThisExpressionSyntax : ExpressionSyntax
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
        internal ThisExpressionSyntax()
            : base(SyntaxToken.This())
        {
            keyword = base.StartToken;
        }

        internal ThisExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(tree, parent, expression)
        {
            keyword = new SyntaxToken(expression.THIS());
        }
                

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            keyword.GetSourceText(writer);
        }
    }
}
