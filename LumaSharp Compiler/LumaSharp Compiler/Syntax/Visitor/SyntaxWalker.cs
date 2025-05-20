
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
            VisitToken(baseExpression.Keyword);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax binaryExpression)
        {
            depth++;
            VisitExpression(binaryExpression.Left);
            {
                VisitToken(binaryExpression.Operation);
            }
            VisitExpression(binaryExpression.Right);
            depth--;
        }

        public override void VisitIndexExpression(IndexExpressionSyntax indexExpression)
        {
            VisitExpression(indexExpression.AccessExpression);
            VisitToken(indexExpression.LArray);
            VisitSyntaxList(indexExpression.IndexExpressions);
            VisitToken(indexExpression.RArray);
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax literalExpression)
        {
            VisitToken(literalExpression.Value);
            if(literalExpression.HasDescriptor == true)
                VisitToken(literalExpression.Descriptor);
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression)
        {
            VisitExpression(memberAccessExpression.AccessExpression);
            VisitToken(memberAccessExpression.Dot);
            VisitToken(memberAccessExpression.Identifier);
        }

        public override void VisitMethodInvokeExpression(MethodInvokeExpressionSyntax methodInvokeExpression)
        {
            VisitExpression(methodInvokeExpression.AccessExpression);
            if (methodInvokeExpression.HasGenericArguments == true)
                VisitGenericArgumentList(methodInvokeExpression.GenericArgumentList);
            VisitArgumentList(methodInvokeExpression.ArgumentList);
        }

        public override void VisitNewExpression(NewExpressionSyntax newExpression)
        {
            VisitToken(newExpression.Keyword);
            VisitTypeReference(newExpression.NewType);
            if(newExpression.HasArguments == true)
                VisitArgumentList(newExpression.ArgumentList);
        }

        public override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            VisitToken(parenthesizedExpression.LParen);
            {
                depth++;
                VisitExpression(parenthesizedExpression.Expression);
                depth--;
            }
            VisitToken(parenthesizedExpression.RParen);
        }

        public override void VisitSizeofExpression(SizeofExpressionSyntax sizeExpression)
        {
            VisitToken(sizeExpression.Keyword);
            VisitToken(sizeExpression.LParen);
            {
                depth++;
                VisitTypeReference(sizeExpression.TypeReference);
                depth--;
            }
            VisitToken(sizeExpression.RParen);
        }

        public override void VisitTernaryExpression(TernaryExpressionSyntax ternaryExpression)
        {
            VisitExpression(ternaryExpression.Condition);
            VisitToken(ternaryExpression.Ternary);
            VisitExpression(ternaryExpression.TrueExpression);
            VisitToken(ternaryExpression.Colon);
            VisitExpression(ternaryExpression.FalseExpression);
        }

        public override void VisitThisExpression(ThisExpressionSyntax thisExpression)
        {
            VisitToken(thisExpression.Keyword);
        }

        public override void VisitTypeofExpression(TypeofExpressionSyntax typeofExpression)
        {
            VisitToken(typeofExpression.Keyword);
            VisitToken(typeofExpression.LParen);
            VisitTypeReference(typeofExpression.TypeReference);
            VisitToken(typeofExpression.RParen);
        }

        public override void VisitUnaryExpression(UnaryExpressionSyntax unaryExpression)
        {
            if(unaryExpression.IsPrefix == true)
            {
                VisitToken(unaryExpression.Operation);
                VisitExpression(unaryExpression.Expression);
            }
            else
            {
                VisitExpression(unaryExpression.Expression);
                VisitToken(unaryExpression.Operation);
            }
        }

        public override void VisitVariableAssignExpression(VariableAssignExpressionSyntax variableAssignExpression)
        {
            VisitToken(variableAssignExpression.Assign);
            VisitSyntaxList(variableAssignExpression.AssignExpressions);
        }

        public override void VisitVariableReferenceExpression(VariableReferenceExpressionSyntax variableReferenceExpression)
        {
            VisitToken(variableReferenceExpression.Identifier);
        }
    }
}
