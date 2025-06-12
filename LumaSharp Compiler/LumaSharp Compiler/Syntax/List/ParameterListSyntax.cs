
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ParameterListSyntax : SeparatedSyntaxList<ParameterSyntax>
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

        public bool HasParameters
        {
            get { return Count > 0; }
        }

        // Constructor
        internal ParameterListSyntax()
            : this(null)
        {
        }

        internal ParameterListSyntax(SeparatedSyntaxList<ParameterSyntax> parameters)
            : this(
                  Syntax.Token(SyntaxTokenKind.LParenSymbol),
                  parameters,
                  Syntax.Token(SyntaxTokenKind.RParenSymbol))
        {
        }

        internal ParameterListSyntax(SyntaxToken lParen, SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken rParen)
            : base(parameters)
        {
            // Check kind
            if(lParen.Kind != SyntaxTokenKind.LParenSymbol)
                throw new ArgumentException(nameof(lParen) + " must be of kind: " + SyntaxTokenKind.LParenSymbol);

            if(rParen.Kind != SyntaxTokenKind.RParenSymbol)
                throw new ArgumentException(nameof(rParen) + " must be of kind: " + SyntaxTokenKind.RParenSymbol);

            this.lParen = lParen;
            this.rParen = rParen;

            // Set parent
            if (parameters != null) parameters.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitParameterList(this);
        }

        public override J Accept<J>(SyntaxVisitor<J> visitor)
        {
            return visitor.VisitParameterList(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Parameter start
            lParen.GetSourceText(writer);

            // Parameters
            base.GetSourceText(writer);

            // Parameter end
            rParen.GetSourceText(writer);
        }
    }
}
