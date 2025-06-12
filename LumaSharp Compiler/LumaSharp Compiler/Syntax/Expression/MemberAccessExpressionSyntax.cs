
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class MemberAccessExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly ExpressionSyntax accessExpression;
        private readonly SyntaxToken dot;
        private readonly SyntaxToken identifier;        

        // Properties
        public override SyntaxToken StartToken
        {
            get { return accessExpression.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return identifier; }
        }

        public ExpressionSyntax AccessExpression
        {
            get { return accessExpression; }
        }

        public SyntaxToken Dot
        {
            get { return dot; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return accessExpression; }
        }

        // Constructor
        internal MemberAccessExpressionSyntax(ExpressionSyntax accessExpression, SyntaxToken identifier)
            : this(
                  accessExpression,
                  Syntax.Token(SyntaxTokenKind.DotSymbol),
                  identifier)
        {
        }

        internal MemberAccessExpressionSyntax(ExpressionSyntax accessExpression, SyntaxToken dot, SyntaxToken identifier)
        {            
            // Check for null
            if(accessExpression == null)
                throw new ArgumentNullException(nameof(accessExpression));

            // Check kind
            if(dot.Kind != SyntaxTokenKind.DotSymbol)
                throw new ArgumentException(nameof(dot) + " must be of kind: " + SyntaxTokenKind.DotSymbol);

            if (identifier.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(identifier) + " must be of kind: " + SyntaxTokenKind.Identifier);

            this.accessExpression = accessExpression;
            this.dot = dot;
            this.identifier = identifier;

            // Set parent
            accessExpression.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitMemberAccessExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitMemberAccessExpression(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write access expression
            accessExpression.GetSourceText(writer);

            // Write dot
            dot.GetSourceText(writer);

            // Write identifier
            writer.Write(identifier.ToString());
        }
    }
}
