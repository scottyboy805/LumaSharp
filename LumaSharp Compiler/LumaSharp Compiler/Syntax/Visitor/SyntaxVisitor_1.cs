
namespace LumaSharp.Compiler.AST.Visitor
{
    public abstract class SyntaxVisitor<T>
    {
        // Methods
        public abstract T VisitToken(SyntaxToken token);
        public abstract T VisitTrivia(SyntaxTrivia trivia);

        public abstract T VisitCompilationUnit(CompilationUnitSyntax compilationUnit);
        public abstract T VisitImport(ImportSyntax import);
        public abstract T VisitNamespace(NamespaceSyntax ns);

        public abstract T VisitTokenList(SeparatedTokenList list);
        public abstract T VisitSyntaxList<J>(SeparatedSyntaxList<J> list) where J : SyntaxNode;
        public abstract T VisitBaseTypeList(BaseTypeListSyntax baseTypeList);
        public abstract T VisitGenericParameterList(GenericParameterListSyntax genericParameterList);
        public abstract T VisitGenericParameter(GenericParameterSyntax genericParameter);
        public abstract T VisitGenericParameterConstraints(GenericParameterConstraintsSyntax genericConstraints);
        public abstract T VisitGenericArgumentList(GenericArgumentListSyntax genericArgumentList);
        public abstract T VisitParameterList(ParameterListSyntax parameterList);
        public abstract T VisitParameter(ParameterSyntax parameter);
        public abstract T VisitArgumentList(ArgumentListSyntax argumentList);
        public abstract T VisitArrayParameters(ArrayParametersSyntax arrayParameterList);
        public abstract T VisitAttribute(AttributeSyntax attribute);

        public abstract T VisitParentTypeReference(ParentTypeReferenceSyntax parentTypeReference);
        public abstract T VisitTypeReference(TypeReferenceSyntax typeReference);
        public abstract T VisitConstructorInvoke(ConstructorInvokeSyntax constructorInvoke);
        public abstract T VisitLambda(LambdaSyntax lambda);
        public abstract T VisitVariableDeclaration(VariableDeclarationSyntax variableDeclaration);

        #region Member
        public abstract T VisitType(TypeSyntax type);
        public abstract T VisitContract(ContractSyntax contract);
        public abstract T VisitEnum(EnumSyntax e);
        public abstract T VisitEnumBlock(EnumBlockSyntax enumBlock);
        public abstract T VisitEnumField(EnumFieldSyntax enumField);
        public abstract T VisitField(FieldSyntax field);
        public abstract T VisitAccessor(AccessorSyntax accessor);
        public abstract T VisitAccessorBody(AccessorBodySyntax accessorBody);
        public abstract T VisitAccessorLambda(AccessorLambdaSyntax accessorLambda);
        public abstract T VisitConstructor(ConstructorSyntax constructor);
        public abstract T VisitMethod(MethodSyntax method);
        public abstract T VisitMemberBlock(MemberBlockSyntax memberBlock);
        #endregion

        #region Statement
        public abstract T VisitAssignStatement(AssignStatementSyntax assignStatement);
        public abstract T VisitBreakStatement(BreakStatementSyntax breakStatement);
        public abstract T VisitConditionStatement(ConditionStatementSyntax conditionStatement);
        public abstract T VisitContinueStatement(ContinueStatementSyntax continueStatement);
        public abstract T VisitEmptyStatement(EmptyStatementSyntax emptyStatement);
        public abstract T VisitForeachStatement(ForeachStatementSyntax foreachStatement);
        public abstract T VisitForStatement(ForStatementSyntax forStatement);
        public abstract T VisitMethodInvokeStatement(MethodInvokeStatementSyntax methodInvokeStatement);
        public abstract T VisitReturnStatement(ReturnStatementSyntax returnStatement);
        public abstract T VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax variableDeclarationStatement);
        public abstract T VisitStatementBlock(StatementBlockSyntax statementBlock);
        #endregion

        #region Expression
        public abstract T VisitAssignExpression(AssignExpressionSyntax assignExpression);
        public abstract T VisitBaseExpression(BaseExpressionSyntax baseExpression);
        public abstract T VisitBinaryExpression(BinaryExpressionSyntax binaryExpression);
        public abstract T VisitCollectionInitializerExpression(CollectionInitializerExpressionSyntax collectionInitializerExpression);
        public abstract T VisitIndexExpression(IndexExpressionSyntax indexExpression);
        public abstract T VisitLiteralExpression(LiteralExpressionSyntax literalExpression);
        public abstract T VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression);
        public abstract T VisitMethodInvokeExpression(MethodInvokeExpressionSyntax methodInvokeExpression);
        public abstract T VisitNewExpression(NewExpressionSyntax newExpression);
        public abstract T VisitParenthesizedExpression(ParenthesizedExpressionSyntax parenthesizedExpression);
        public abstract T VisitRangeExpression(RangeExpressionSyntax rangeExpression);
        public abstract T VisitSizeofExpression(SizeofExpressionSyntax sizeExpression);
        public abstract T VisitTernaryExpression(TernaryExpressionSyntax ternaryExpression);
        public abstract T VisitThisExpression(ThisExpressionSyntax thisExpression);
        public abstract T VisitTypeofExpression(TypeofExpressionSyntax typeofExpression);
        public abstract T VisitUnaryExpression(UnaryExpressionSyntax unaryExpression);
        public abstract T VisitVariableAssignmentExpression(VariableAssignmentExpressionSyntax variableAssignExpression);
        public abstract T VisitVariableReferenceExpression(VariableReferenceExpressionSyntax variableReferenceExpression);
        #endregion
    }
}
