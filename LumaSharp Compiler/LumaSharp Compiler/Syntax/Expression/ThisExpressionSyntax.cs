
namespace LumaSharp_Compiler.Syntax
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

        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return keyword; }
        }

        // Constructor
        internal ThisExpressionSyntax(SyntaxTree tree, SyntaxNode parent)
            : base(tree, parent)
        {
            keyword = new SyntaxToken("this");
        }
                

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            writer.Write(keyword.ToString());
        }
    }
}
