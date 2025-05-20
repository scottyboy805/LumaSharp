
namespace LumaSharp.Compiler.AST.Visitor
{
    public abstract class SyntaxVisitor
    {
        public virtual void VisitToken(SyntaxToken token) { }
        public virtual void VisitTrivia(SyntaxTrivia trivia) { }

        public virtual void VisitTokenList(SeparatedTokenList list) { }
        public virtual void VisitSyntaxList<T>(SeparatedSyntaxList<T> list) where T : SyntaxNode { }
        public virtual void VisitGenericParameterList(GenericParameterListSyntax genericParameterList) { }
        public virtual void VisitGenericArgumentList(GenericArgumentListSyntax genericArgumentList) { }
        public virtual void VisitParameterList(ParameterListSyntax parameterList) { }
        public virtual void VisitArgumentList(ArgumentListSyntax argumentList) { }

        public virtual void VisitTypeReference(TypeReferenceSyntax typeReference) { }

        #region Expression
        public virtual void VisitExpression(ExpressionSyntax expression)
        {
            // Accept the visit
            expression.Accept(this);
        }

        public virtual void VisitBaseExpression(BaseExpressionSyntax baseExpression) { }
        public virtual void VisitBinaryExpression(BinaryExpressionSyntax binaryExpression) { }
        public virtual void VisitIndexExpression(IndexExpressionSyntax indexExpression) { }
        public virtual void VisitLiteralExpression(LiteralExpressionSyntax literalExpression) { }
        public virtual void VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression) { }
        public virtual void VisitMethodInvokeExpression(MethodInvokeExpressionSyntax methodInvokeExpression) { }
        public virtual void VisitNewExpression(NewExpressionSyntax newExpression) { }
        public virtual void VisitParenthesizedExpression(ParenthesizedExpressionSyntax parenthesizedExpression) { }
        public virtual void VisitSizeofExpression(SizeofExpressionSyntax sizeExpression) { }
        public virtual void VisitTernaryExpression(TernaryExpressionSyntax ternaryExpression) { }
        public virtual void VisitThisExpression(ThisExpressionSyntax thisExpression) { }
        public virtual void VisitTypeofExpression(TypeofExpressionSyntax typeofExpression) { }
        public virtual void VisitUnaryExpression(UnaryExpressionSyntax unaryExpression) { }
        public virtual void VisitVariableAssignExpression(VariableAssignExpressionSyntax variableAssignExpression) { }
        public virtual void VisitVariableReferenceExpression(VariableReferenceExpressionSyntax variableReferenceExpression) { }
        #endregion
    }
}
