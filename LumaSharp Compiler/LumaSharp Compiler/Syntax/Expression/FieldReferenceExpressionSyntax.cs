
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
        internal FieldReferenceExpressionSyntax(SyntaxToken identifier, ExpressionSyntax accessExpression)
        {
            this.identifier = identifier;
            this.accessExpression = accessExpression;
            this.dot = Syntax.Token(SyntaxTokenKind.DotSymbol);
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
