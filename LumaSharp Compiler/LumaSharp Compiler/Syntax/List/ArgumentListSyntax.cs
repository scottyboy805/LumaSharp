
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public class ArgumentListSyntax : SeparatedSyntaxList<ExpressionSyntax>
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
        internal ArgumentListSyntax()
            : this(null)
        {
        }

        internal ArgumentListSyntax(SeparatedSyntaxList<ExpressionSyntax> argumentExpressions)
            : this(
                  Syntax.Token(SyntaxTokenKind.LParenSymbol),
                  argumentExpressions,
                  Syntax.Token(SyntaxTokenKind.RParenSymbol))
        {
        }

        internal ArgumentListSyntax(SyntaxToken lParen, SeparatedSyntaxList<ExpressionSyntax> argumentExpressions, SyntaxToken rParen)
            : base(argumentExpressions)
        {
            // Check kind
            if (lParen.Kind != SyntaxTokenKind.LParenSymbol)
                throw new ArgumentException(nameof(lParen) + " must be of kind: " + SyntaxTokenKind.LParenSymbol);

            if(rParen.Kind != SyntaxTokenKind.RParenSymbol)
                throw new ArgumentException(nameof(rParen) + " must be of kind: " + SyntaxTokenKind.RParenSymbol);

            this.lParen = lParen;
            this.rParen = rParen;

            // Set parent
            if(argumentExpressions != null) argumentExpressions.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitArgumentList(this);
        }

        public override J Accept<J>(SyntaxVisitor<J> visitor)
        {
            return visitor.VisitArgumentList(this);
        }

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
