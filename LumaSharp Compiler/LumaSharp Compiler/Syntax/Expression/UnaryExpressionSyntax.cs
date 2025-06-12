
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public enum UnaryOperation
    {
        PrefixNegate,
        PrefixNot,
        PrefixAdd,
        PrefixSubtract,
        PostfixAdd,
        PostfixSubtract,
    }

    public sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken operation;
        private readonly ExpressionSyntax expression;
        private readonly bool isPrefix;

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                // Check for prefix
                if(isPrefix == true)
                    return operation;

                return expression.StartToken;
            }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for prefix
                if (isPrefix == false)
                    return operation;

                return expression.EndToken;
            }
        }

        public SyntaxToken Operation
        {
            get { return operation; }
        }

        public ExpressionSyntax Expression
        {
            get { return expression; }
        }

        public bool IsPrefix
        {
            get { return isPrefix; }
        }

        public UnaryOperation UnaryOperation
        {
            get
            {
                return operation.Text switch
                {
                    "-" => UnaryOperation.PrefixNegate,
                    "!" => UnaryOperation.PrefixNot,
                    "++" => isPrefix ? UnaryOperation.PrefixAdd : UnaryOperation.PostfixAdd,
                    "--" => isPrefix ? UnaryOperation.PrefixSubtract : UnaryOperation.PostfixSubtract,

                    _ => throw new NotImplementedException(),
                };
            }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return expression; }
        }

        // Constructor
        internal UnaryExpressionSyntax(ExpressionSyntax expression, SyntaxToken op, bool isPrefix)
        {
            // Check for null
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            // Check for unary
            if (op.IsUnaryOperand == false)
                throw new ArgumentException(nameof(op) + " must be a valid unary operator");

            this.operation = op;
            this.expression = expression;
            this.isPrefix = isPrefix;

            // Set parent
            expression.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitUnaryExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Check for prefix
            if(isPrefix == true)
            {
                operation.GetSourceText(writer);
                expression.GetSourceText(writer);
            }
            else
            {
                expression.GetSourceText(writer);
                operation.GetSourceText(writer);
            }
        }
    }
}
