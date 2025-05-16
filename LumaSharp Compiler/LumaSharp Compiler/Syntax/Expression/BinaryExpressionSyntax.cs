
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
        public override SyntaxToken StartToken => left.StartToken;
        public override SyntaxToken EndToken => right.EndToken;
        public SyntaxToken Operation => operation;
        public ExpressionSyntax Left => left;
        public ExpressionSyntax Right => right;

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
        internal BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken op, ExpressionSyntax right)
        {
            // Check null
            if(left == null)
                throw new ArgumentNullException(nameof(left));

            if(right == null)
                throw new ArgumentNullException(nameof(right));

            // Check for binary
            if (op.IsBinaryOperand == false)
                throw new ArgumentException(nameof(op) + " must be a valid binary operator");

            this.left = left;
            this.right = right;
            this.operation = op;
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
