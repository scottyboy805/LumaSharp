
namespace LumaSharp.Compiler.AST
{
    public sealed class FieldReferenceExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly ExpressionSyntax accessExpression;
        private readonly SyntaxToken identifier;
        private readonly SyntaxToken dot;

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

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public SyntaxToken Dot
        {
            get { return dot; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return accessExpression; }
        }

        // Constructor
        internal FieldReferenceExpressionSyntax(SyntaxNode parent, string identifier, ExpressionSyntax accessExpression)
            : base(parent, null)
        {
            this.identifier = Syntax.Identifier(identifier);
            this.accessExpression = accessExpression;
            this.dot = Syntax.KeywordOrSymbol(SyntaxTokenKind.DotSymbol);
        }

        internal FieldReferenceExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            // Identifier
            this.identifier = new SyntaxToken(SyntaxTokenKind.Identifier, expression.fieldAccessExpression().IDENTIFIER());

            // Dot
            this.dot = new SyntaxToken(SyntaxTokenKind.DotSymbol, expression.fieldAccessExpression().DOT());

            // Create access expression
            this.accessExpression = Any(this, expression.expression(0));
        }

        // Methods
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
