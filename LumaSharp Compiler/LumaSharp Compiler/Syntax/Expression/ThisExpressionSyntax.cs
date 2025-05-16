
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
        internal ThisExpressionSyntax()
            : this(
                  new SyntaxToken(SyntaxTokenKind.ThisKeyword))
        {
        }

        internal ThisExpressionSyntax(SyntaxToken thisToken)
        {
            // Check for this
            if (thisToken.Kind != SyntaxTokenKind.ThisKeyword)
                throw new ArgumentException(nameof(thisToken) + " must be of kind: " + SyntaxTokenKind.ThisKeyword);

            keyword = thisToken;
        }
                

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            keyword.GetSourceText(writer);
        }
    }
}
