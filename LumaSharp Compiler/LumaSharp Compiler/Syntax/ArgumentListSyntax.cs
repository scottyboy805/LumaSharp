
namespace LumaSharp.Compiler.AST
{
    public class ArgumentListSyntax : SeparatedListSyntax<ExpressionSyntax>
    {
        // Private
        private readonly SyntaxToken lParen;
        private readonly SyntaxToken rParen;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lParen; }
        }

        public override SyntaxToken EndToken
        {
            get { return rParen; }
        }

        public SyntaxToken LParen
        {
            get { return lParen; }
        }

        public SyntaxToken RParen
        {
            get { return rParen; }
        }

        public bool HasArgumentExpressions
        {
            get { return Count > 0; }
        }

        // Constructor
        internal ArgumentListSyntax(SeparatedListSyntax<ExpressionSyntax> argumentExpressions)
            : this(
                  new SyntaxToken(SyntaxTokenKind.LParenSymbol),
                  argumentExpressions,
                  new SyntaxToken(SyntaxTokenKind.RParenSymbol))
        {
        }

        internal ArgumentListSyntax(SyntaxToken lParen, SeparatedListSyntax<ExpressionSyntax> argumentExpressions, SyntaxToken rParen)
            : base(argumentExpressions)
        {
            // Check kind
            if (lParen.Kind != SyntaxTokenKind.LParenSymbol)
                throw new ArgumentException(nameof(lParen) + " must be of kind: " + SyntaxTokenKind.LParenSymbol);

            if(rParen.Kind != SyntaxTokenKind.RParenSymbol)
                throw new ArgumentException(nameof(rParen) + " must be of kind: " + SyntaxTokenKind.RParenSymbol);

            this.lParen = lParen;
            this.rParen = rParen;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Argument start
            lParen.GetSourceText(writer);

            // Arguments
            base.GetSourceText(writer);

            // Argument end
            rParen.GetSourceText(writer);
        }
    }
}
