
namespace LumaSharp.Compiler.AST
{
    public sealed class BaseExpressionSyntax : ExpressionSyntax
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
        internal BaseExpressionSyntax(SyntaxNode parent)
            : base(parent, null)
        {
            this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.BaseKeyword);
        }

        internal BaseExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            keyword = new SyntaxToken(SyntaxTokenKind.BaseKeyword, expression.BASE());
        }


        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write source
            keyword.GetSourceText(writer);
        }
    }
}
