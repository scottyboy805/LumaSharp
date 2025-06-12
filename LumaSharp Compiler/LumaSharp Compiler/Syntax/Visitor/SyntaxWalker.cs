
namespace LumaSharp.Compiler.AST.Visitor
{
    public abstract class SyntaxWalker : SyntaxVisitor
    {
        // Private
        private int depth = 0;

        // Properties
        public int Depth => depth;

        // Methods
        public override void VisitBaseExpression(BaseExpressionSyntax baseExpression)
        {
            baseExpression.Keyword.Accept(this);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax binaryExpression)
        {
            depth++;
            binaryExpression.Left.Accept(this);
            {
                binaryExpression.Operation.Accept(this);
            }
            binaryExpression.Right.Accept(this);
            depth--;
        }

        public override void VisitIndexExpression(IndexExpressionSyntax indexExpression)
        {
            indexExpression.AccessExpression.Accept(this);
            indexExpression.LArray.Accept(this);
            indexExpression.IndexExpressions.Accept(this);
            indexExpression.RArray.Accept(this);
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax literalExpression)
        {
            VisitToken(literalExpression.Value);
            if(literalExpression.Descriptor != null)
                VisitToken(literalExpression.Descriptor.Value);
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression)
        {
            memberAccessExpression.AccessExpression.Accept(this);
            memberAccessExpression.Dot.Accept(this);
            memberAccessExpression.Identifier.Accept(this);
        }

        public override void VisitMethodInvokeExpression(MethodInvokeExpressionSyntax methodInvokeExpression)
        {
            methodInvokeExpression.AccessExpression.Accept(this);
            if (methodInvokeExpression.HasGenericArguments == true)
                methodInvokeExpression.GenericArgumentList.Accept(this);
            methodInvokeExpression.ArgumentList.Accept(this);
        }

        public override void VisitNewExpression(NewExpressionSyntax newExpression)
        {
            newExpression.Keyword.Accept(this);
            newExpression.NewType.Accept(this);
            if(newExpression.HasArguments == true)
                newExpression.ArgumentList.Accept(this);
        }

        public override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            parenthesizedExpression.LParen.Accept(this);
            {
                depth++;
                parenthesizedExpression.Expression.Accept(this);
                depth--;
            }
            parenthesizedExpression.RParen.Accept(this);
        }

        public override void VisitSizeofExpression(SizeofExpressionSyntax sizeExpression)
        {
            sizeExpression.Keyword.Accept(this);
            sizeExpression.LParen.Accept(this);
            {
                depth++;
                sizeExpression.TypeReference.Accept(this);
                depth--;
            }
            sizeExpression.RParen.Accept(this);
        }

        public override void VisitTernaryExpression(TernaryExpressionSyntax ternaryExpression)
        {
            ternaryExpression.Condition.Accept(this);
            ternaryExpression.Ternary.Accept(this);
            ternaryExpression.TrueExpression.Accept(this);
            ternaryExpression.Colon.Accept(this);
            ternaryExpression.FalseExpression.Accept(this);
        }

        public override void VisitThisExpression(ThisExpressionSyntax thisExpression)
        {
            thisExpression.Keyword.Accept(this);
        }

        public override void VisitTypeofExpression(TypeofExpressionSyntax typeofExpression)
        {
            typeofExpression.Keyword.Accept(this);
            typeofExpression.LParen.Accept(this);
            typeofExpression.TypeReference.Accept(this);
            typeofExpression.RParen.Accept(this);
        }

        public override void VisitUnaryExpression(UnaryExpressionSyntax unaryExpression)
        {
            if(unaryExpression.IsPrefix == true)
            {
                unaryExpression.Operation.Accept(this);
                unaryExpression.Expression.Accept(this);
            }
            else
            {
                unaryExpression.Expression.Accept(this);
                unaryExpression.Operation.Accept(this);
            }
        }

        public override void VisitVariableAssignmentExpression(VariableAssignmentExpressionSyntax variableAssignExpression)
        {
            variableAssignExpression.Assign.Accept(this);
            variableAssignExpression.AssignExpressions.Accept(this);
        }

        public override void VisitVariableReferenceExpression(VariableReferenceExpressionSyntax variableReferenceExpression)
        {
            variableReferenceExpression.Identifier.Accept(this);
        }
    }
}
