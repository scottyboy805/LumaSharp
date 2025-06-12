
using LumaSharp.Compiler.AST.Visitor;

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

            // Set parent
            left.parent = this;
            right.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitBinaryExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }

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
