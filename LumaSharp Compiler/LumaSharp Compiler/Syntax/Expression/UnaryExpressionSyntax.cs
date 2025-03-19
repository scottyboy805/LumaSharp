
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
        internal UnaryExpressionSyntax(SyntaxNode parent, UnaryOperation op, ExpressionSyntax expression)
            : base(parent, null)
        {
            this.operation = Syntax.UnaryOp(op);
            this.expression = expression;
            this.isPrefix = op != UnaryOperation.PostfixAdd && op != UnaryOperation.PostfixSubtract;
        }

        internal UnaryExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            this.expression = ExpressionSyntax.Any(this, expression.expression(0));

            // Check for prefix
            if(expression.unaryPrefix != null)
            {
                this.operation = new SyntaxToken(SyntaxTokenKind.UnaryOperator, expression.unaryPrefix);
                this.isPrefix = true;
            }
            else
            {
                this.operation = new SyntaxToken(SyntaxTokenKind.UnaryOperator, expression.unaryPostfix);
                this.isPrefix = false;
            }
        }

        // Methods
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
