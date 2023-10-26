
using Antlr4.Runtime;

namespace LumaSharp_Compiler.AST
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

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return left;
                yield return right;
            }
        }

        // Constructor
        public BinaryExpressionSyntax(ExpressionSyntax left, BinaryOperation op, ExpressionSyntax right)
            : base(left.StartToken, right.EndToken)
        {
            this.left = left;
            this.right = right;

            this.operation = GetBinaryOperation(op);
        }

        internal BinaryExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(tree, parent, expression)
        {
            // Op
            this.operation = new SyntaxToken(expression.binary);

            // Get left
            this.left = Any(tree, this, expression.expression(0));

            // Get right
            this.right = Any(tree, this, expression.expression(1));
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

        private static SyntaxToken GetBinaryOperation(BinaryOperation op)
        {
            return new SyntaxToken(op switch
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
    }
}
