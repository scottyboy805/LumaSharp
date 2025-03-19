
namespace LumaSharp.Compiler.AST
{
    public enum BinaryOperation
    {
        Add,
        Subtract, 
        Multiply,
        Divide,
        Modulus,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,
        Equal,
        NotEqual,
        And,
        Or
    }

    public sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken operation;
        private readonly ExpressionSyntax left;        
        private readonly ExpressionSyntax right;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return left.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return right.EndToken; }
        }

        public SyntaxToken Operation
        {
            get { return operation; }
        }

        public ExpressionSyntax Left
        {
            get { return left; }
        }

        public ExpressionSyntax Right
        {
            get { return right; }
        }

        public BinaryOperation BinaryOperation
        {
            get
            {
                return operation.Text switch
                {
                    "+" => BinaryOperation.Add,
                    "-" => BinaryOperation.Subtract,
                    "*" => BinaryOperation.Multiply,
                    "/" => BinaryOperation.Divide,
                    "%" => BinaryOperation.Modulus,
                    ">" => BinaryOperation.Greater,
                    ">=" => BinaryOperation.GreaterEqual,
                    "<" => BinaryOperation.Less,
                    "<=" => BinaryOperation.LessEqual,
                    "==" => BinaryOperation.Equal,
                    "!=" => BinaryOperation.NotEqual,
                    "&&" => BinaryOperation.And,
                    "||" => BinaryOperation.Or,

                    _ => throw new NotImplementedException(),
                };
            }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return left;
                yield return right;
            }
        }

        // Constructor
        internal BinaryExpressionSyntax(SyntaxNode parent, ExpressionSyntax left, BinaryOperation op, ExpressionSyntax right)
            : base(parent, null)
        {
            this.left = left;
            this.right = right;
            this.operation = Syntax.BinaryOp(op);
        }

        internal BinaryExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            // Op
            this.operation = new SyntaxToken(SyntaxTokenKind.BinaryOperator, expression.binary);

            // Get left
            this.left = Any(this, expression.expression(0));

            // Get right
            this.right = Any(this, expression.expression(1));
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write left
            left.GetSourceText(writer);

            // Write operation
            writer.Write(operation.ToString());

            // Write right
            right.GetSourceText(writer);
        }
    }
}
