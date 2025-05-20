
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class TernaryExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly ExpressionSyntax condition;
        private readonly ExpressionSyntax trueExpression;
        private readonly ExpressionSyntax falseExpression;
        private readonly SyntaxToken ternary;
        private readonly SyntaxToken colon;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return condition.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return falseExpression.EndToken; }
        }

        public ExpressionSyntax Condition
        {
            get { return condition; }
        }

        public ExpressionSyntax TrueExpression
        {
            get { return trueExpression; }
        }

        public ExpressionSyntax FalseExpression
        {
            get { return falseExpression; }
        }

        public SyntaxToken Ternary
        {
            get { return ternary; }
        }

        public SyntaxToken Colon
        {
            get { return colon; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return condition;
                yield return trueExpression;
                yield return falseExpression;
            }
        }

        // Constructor
        internal TernaryExpressionSyntax(ExpressionSyntax condition, ExpressionSyntax trueExpression, ExpressionSyntax falseExpression)
            : this(
                  condition,
                  new SyntaxToken(SyntaxTokenKind.TernarySymbol),
                  trueExpression,
                  new SyntaxToken(SyntaxTokenKind.ColonSymbol),
                  falseExpression)
        {
        }

        internal TernaryExpressionSyntax(ExpressionSyntax condition, SyntaxToken ternary, ExpressionSyntax trueExpression, SyntaxToken colon, ExpressionSyntax falseExpression)
        {
            // Check for null
            if(condition == null)
                throw new ArgumentNullException(nameof(condition));

            if(trueExpression == null)
                throw new ArgumentNullException(nameof(trueExpression));

            if(falseExpression == null)
                throw new ArgumentNullException(nameof(falseExpression));

            // Check kind
            if (ternary.Kind != SyntaxTokenKind.TernarySymbol)
                throw new ArgumentException(nameof(ternary) + " must be of kind: " + SyntaxTokenKind.TernarySymbol);

            if(colon.Kind != SyntaxTokenKind.ColonSymbol)
                throw new ArgumentException(nameof(colon) + " must be of kind: " + SyntaxTokenKind.ColonSymbol);

            this.condition = condition;
            this.ternary = ternary;
            this.trueExpression = trueExpression;
            this.colon = colon;
            this.falseExpression = falseExpression;

            // Set parent
            condition.parent = this;
            trueExpression.parent = this;
            falseExpression.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write condition
            condition.GetSourceText(writer);

            // Ternary
            ternary.GetSourceText(writer);

            // Write true
            trueExpression.GetSourceText(writer);

            // Write alternate
            colon.GetSourceText(writer);

            // Write false
            falseExpression.GetSourceText(writer);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitTernaryExpression(this);
        }
    }
}
