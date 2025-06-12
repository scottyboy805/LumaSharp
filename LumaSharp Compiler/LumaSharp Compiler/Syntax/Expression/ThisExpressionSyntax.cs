
using LumaSharp.Compiler.AST.Visitor;

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
                  Syntax.Token(SyntaxTokenKind.ThisKeyword))
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
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitThisExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitThisExpression(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            keyword.GetSourceText(writer);
        }
    }
}
