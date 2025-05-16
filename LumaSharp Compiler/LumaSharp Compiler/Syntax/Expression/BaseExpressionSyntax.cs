
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
        internal BaseExpressionSyntax()
            : this(
                  new SyntaxToken(SyntaxTokenKind.BaseKeyword))
        {
        }

        internal BaseExpressionSyntax(SyntaxToken keyword)
        {
            // Check for this
            if (keyword.Kind != SyntaxTokenKind.BaseKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.BaseKeyword);

            this.keyword = keyword;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write source
            keyword.GetSourceText(writer);
        }
    }
}
