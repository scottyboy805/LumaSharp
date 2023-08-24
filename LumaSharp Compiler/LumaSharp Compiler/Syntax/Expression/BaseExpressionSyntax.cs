
namespace LumaSharp_Compiler.Syntax
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

        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return keyword; }
        }

        // Constructor
        internal BaseExpressionSyntax()
        {
            keyword = new SyntaxToken("base");
        }


        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            writer.Write(keyword.ToString());
        }
    }
}
