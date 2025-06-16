
namespace LumaSharp.Compiler.AST.Visitor
{
    public abstract class SyntaxVisitor
    {
        // Methods
        public virtual void VisitToken(SyntaxToken token) { }
        public virtual void VisitTrivia(SyntaxTrivia trivia) { }

        public virtual void VisitCompilationUnit(CompilationUnitSyntax compilationUnit) { }
        public virtual void VisitImport(ImportSyntax import) { }
        public virtual void VisitImpotAlias(ImportAliasSyntax importAlias) { }
        public virtual void VisitNamespace(NamespaceSyntax ns) { }

        public virtual void VisitTokenList(SeparatedTokenList list) { }
        public virtual void VisitSyntaxList<T>(SeparatedSyntaxList<T> list) where T : SyntaxNode { }
        public virtual void VisitBaseTypeList(BaseTypeListSyntax baseTypeList) { }
        public virtual void VisitGenericParameterList(GenericParameterListSyntax genericParameterList) { }
        public virtual void VisitGenericParameter(GenericParameterSyntax genericParameter) { }
        public virtual void VisitGenericParameterConstraints(GenericParameterConstraintsSyntax genericConstraints) { }
        public virtual void VisitGenericArgumentList(GenericArgumentListSyntax genericArgumentList) { }
        public virtual void VisitParameterList(ParameterListSyntax parameterList) { }
        public virtual void VisitParameter(ParameterSyntax parameter) { }
        public virtual void VisitArgumentList(ArgumentListSyntax argumentList) { }
        public virtual void VisitArrayParameters(ArrayParametersSyntax arrayParameterList) { }
        public virtual void VisitAttribute(AttributeSyntax attribute) { }

        public virtual void VisitParentTypeReference(ParentTypeReferenceSyntax parentTypeReference) { }
        public virtual void VisitTypeReference(TypeReferenceSyntax typeReference) { }
        public virtual void VisitConstructorInvoke(ConstructorInvokeSyntax constructorInvoke) { }
        public virtual void VisitLambda(LambdaSyntax lambda) { }
        public virtual void VisitVariableDeclaration(VariableDeclarationSyntax variableDeclaration) { }

        #region Member
        public virtual void VisitType(TypeSyntax type) { }
        public virtual void VisitContract(ContractSyntax contract) { }
        public virtual void VisitEnum(EnumSyntax e) { }
        public virtual void VisitEnumBlock(EnumBlockSyntax enumBlock) { }
        public virtual void VisitEnumField(EnumFieldSyntax enumField) { }
        public virtual void VisitField(FieldSyntax field) { }
        public virtual void VisitAccessor(AccessorSyntax accessor) { }
        public virtual void VisitAccessorBody(AccessorBodySyntax accessorBody) { }
        public virtual void VisitAccessorLambda(AccessorLambdaSyntax accessorLambda) { }
        public virtual void VisitConstructor(ConstructorSyntax constructor) { }
        public virtual void VisitMethod(MethodSyntax method) { }
        public virtual void VisitMemberBlock(MemberBlockSyntax memberBlock) { }
        #endregion

        #region Statement
        public virtual void VisitAssignStatement(AssignStatementSyntax assignStatement) { }
        public virtual void VisitBreakStatement(BreakStatementSyntax breakStatement) { }
        public virtual void VisitConditionStatement(ConditionStatementSyntax conditionStatement) { }
        public virtual void VisitContinueStatement(ContinueStatementSyntax continueStatement) { }
        public virtual void VisitEmptyStatement(EmptyStatementSyntax emptyStatement) { }
        public virtual void VisitForeachStatement(ForeachStatementSyntax foreachStatement) { }
        public virtual void VisitForStatement(ForStatementSyntax forStatement) { }
        public virtual void VisitMethodInvokeStatement(MethodInvokeStatementSyntax methodInvokeStatement) { }
        public virtual void VisitReturnStatement(ReturnStatementSyntax returnStatement) { }
        public virtual void VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax variableDeclarationStatement) { }
        public virtual void VisitStatementBlock(StatementBlockSyntax statementBlock) { }
        #endregion

        #region Expression
        public virtual void VisitAssignExpression(AssignExpressionSyntax assignExpression) { }
        public virtual void VisitBaseExpression(BaseExpressionSyntax baseExpression) { }
        public virtual void VisitBinaryExpression(BinaryExpressionSyntax binaryExpression) { }
        public virtual void VisitCollectionInitializerExpression(CollectionInitializerExpressionSyntax collectionInitializerExpression) { }
        public virtual void VisitIndexExpression(IndexExpressionSyntax indexExpression) { }
        public virtual void VisitLiteralExpression(LiteralExpressionSyntax literalExpression) { }
        public virtual void VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression) { }
        public virtual void VisitMethodInvokeExpression(MethodInvokeExpressionSyntax methodInvokeExpression) { }
        public virtual void VisitNewExpression(NewExpressionSyntax newExpression) { }
        public virtual void VisitParenthesizedExpression(ParenthesizedExpressionSyntax parenthesizedExpression) { }
        public virtual void VisitRangeExpression(RangeExpressionSyntax rangeExpression) { }
        public virtual void VisitSizeofExpression(SizeofExpressionSyntax sizeExpression) { }
        public virtual void VisitTernaryExpression(TernaryExpressionSyntax ternaryExpression) { }
        public virtual void VisitThisExpression(ThisExpressionSyntax thisExpression) { }
        public virtual void VisitTypeofExpression(TypeofExpressionSyntax typeofExpression) { }
        public virtual void VisitUnaryExpression(UnaryExpressionSyntax unaryExpression) { }
        public virtual void VisitVariableAssignmentExpression(VariableAssignmentExpressionSyntax variableAssignExpression) { }
        public virtual void VisitVariableReferenceExpression(VariableReferenceExpressionSyntax variableReferenceExpression) { }
        #endregion
    }
}
