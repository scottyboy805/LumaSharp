
namespace LumaSharp.Compiler.AST
{
    public sealed class ThisExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken keyword;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return keyword; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal ThisExpressionSyntax(SyntaxNode parent)
            : base(parent, null)
        {
            keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.ThisKeyword);
        }

        internal ThisExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            keyword = new SyntaxToken(SyntaxTokenKind.ThisKeyword, expression.THIS());
        }
                

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            keyword.GetSourceText(writer);
        }
    }
}
