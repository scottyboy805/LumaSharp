using System.Text;

namespace LumaSharp.Compiler.AST.Visitor
{
    public class PrintSyntaxTreeWalker : SyntaxWalker
    {
        // Private
        private readonly StringWriter writer = new();

        // Properties
        public string Text => writer.ToString();

        // Methods
        public override void VisitBaseExpression(BaseExpressionSyntax baseExpression)
        {
            AppendLine(nameof(BaseExpressionSyntax));
            base.VisitBaseExpression(baseExpression);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax binaryExpression)
        {
            AppendLine(nameof(BinaryExpressionSyntax) + " [{0}]", binaryExpression.Operation);
            base.VisitBinaryExpression(binaryExpression);
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax literalExpression)
        {
            AppendLine(nameof(LiteralExpressionSyntax) + " ({0})", literalExpression.ToString());
            base.VisitLiteralExpression(literalExpression);
        }

        public override void VisitVariableReferenceExpression(VariableReferenceExpressionSyntax variableReferenceExpression)
        {
            AppendLine(nameof(VariableReferenceExpressionSyntax) + " [{0}]", variableReferenceExpression.Identifier);
            base.VisitVariableReferenceExpression(variableReferenceExpression);
        }

        private void AppendLine(string format, params object[] args) 
        {
            writer.Write(new string('\t', Depth));
            writer.Write(format, args);
            writer.WriteLine();
        }
    }
}
