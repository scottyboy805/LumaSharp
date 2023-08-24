
namespace LumaSharp_Compiler.Syntax
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
        private SyntaxToken operation = null;
        private ExpressionSyntax left = null;        
        private ExpressionSyntax right = null;

        // Properties
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

        public override SyntaxToken StartToken
        {
            get { return left.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return right.EndToken; }
        }

        // Constructor
        internal BinaryExpressionSyntax(ExpressionSyntax left, BinaryOperation op, ExpressionSyntax right)
        {
            this.left = left;
            this.right = right;

            this.operation = new SyntaxToken(op switch
            {
                BinaryOperation.Add => "+",
                BinaryOperation.Subtract => "-",
                BinaryOperation.Multiply => "*",
                BinaryOperation.Divide => "/",
                BinaryOperation.Modulus => "%",
                BinaryOperation.Greater => ">",
                BinaryOperation.GreaterEqual => ">=",
                BinaryOperation.Less => "<",
                BinaryOperation.LessEqual => "<=",
                BinaryOperation.Equal => "==",
                BinaryOperation.NotEqual => "!=",
                BinaryOperation.And => "&&",
                BinaryOperation.Or => "||",
            });
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
