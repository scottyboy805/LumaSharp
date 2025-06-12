
namespace LumaSharp.Compiler.AST.Visitor
{
    public  class SyntaxRewriter : SyntaxVisitor<SyntaxNode>
    {
        // Methods
        public T DefaultVisit<T>(T node) where T : SyntaxNode
        {
            // Check for null
            if (node == null)
                return null;

            // Accept the node
            SyntaxNode result = node.Accept(this);

            // Check for modified
            if (result == node)
                return node;

            // Try to get as target
            return result as T;
        }

        public override SyntaxNode VisitAccessor(AccessorSyntax accessor) => accessor;
        public override SyntaxNode VisitAccessorBody(AccessorBodySyntax accessorBody) => accessorBody;
        public override SyntaxNode VisitAccessorLambda(AccessorLambdaSyntax accessorLambda) => accessorLambda;
        public override SyntaxNode VisitArgumentList(ArgumentListSyntax argumentList) => argumentList;
        public override SyntaxNode VisitArrayParameters(ArrayParametersSyntax arrayParameterList) => arrayParameterList;
        public override SyntaxNode VisitAssignExpression(AssignExpressionSyntax assignExpression) => assignExpression;
        public override SyntaxNode VisitAssignStatement(AssignStatementSyntax assignStatement) => assignStatement;
        public override SyntaxNode VisitAttribute(AttributeSyntax attribute) => attribute;
        public override SyntaxNode VisitBaseExpression(BaseExpressionSyntax baseExpression) => baseExpression;
        public override SyntaxNode VisitBaseTypeList(BaseTypeListSyntax baseTypeList) => baseTypeList;
        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax binaryExpression) => binaryExpression;
        public override SyntaxNode VisitBreakStatement(BreakStatementSyntax breakStatement) => breakStatement;
        public override SyntaxNode VisitCollectionInitializerExpression(CollectionInitializerExpressionSyntax collectionInitializerExpression) => collectionInitializerExpression;
        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax compilationUnit) => compilationUnit;
        public override SyntaxNode VisitConditionStatement(ConditionStatementSyntax conditionStatement) => conditionStatement;
        public override SyntaxNode VisitConstructor(ConstructorSyntax constructor) => constructor;
        public override SyntaxNode VisitConstructorInvoke(ConstructorInvokeSyntax constructorInvoke) => constructorInvoke;
        public override SyntaxNode VisitContinueStatement(ContinueStatementSyntax continueStatement) => continueStatement;
        public override SyntaxNode VisitContract(ContractSyntax contract) => contract;
        public override SyntaxNode VisitEmptyStatement(EmptyStatementSyntax emptyStatement) => emptyStatement;
        public override SyntaxNode VisitEnum(EnumSyntax e) => e;
        public override SyntaxNode VisitEnumBlock(EnumBlockSyntax enumBlock) => enumBlock;
        public override SyntaxNode VisitEnumField(EnumFieldSyntax enumField) => enumField;
        public override SyntaxNode VisitField(FieldSyntax field) => field;
        public override SyntaxNode VisitForeachStatement(ForeachStatementSyntax foreachStatement) => foreachStatement;
        public override SyntaxNode VisitForStatement(ForStatementSyntax forStatement) => forStatement;
        public override SyntaxNode VisitGenericArgumentList(GenericArgumentListSyntax genericArgumentList) => genericArgumentList;
        public override SyntaxNode VisitGenericParameter(GenericParameterSyntax genericParameter) => genericParameter;
        public override SyntaxNode VisitGenericParameterList(GenericParameterListSyntax genericParameterList) => genericParameterList;
        public override SyntaxNode VisitImport(ImportSyntax import) => import;
        public override SyntaxNode VisitIndexExpression(IndexExpressionSyntax indexExpression) => indexExpression;
        public override SyntaxNode VisitLambda(LambdaSyntax lambda) => lambda;
        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax literalExpression) => literalExpression;
        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression) => memberAccessExpression;
        public override SyntaxNode VisitMemberBlock(MemberBlockSyntax memberBlock) => memberBlock;
        public override SyntaxNode VisitMethod(MethodSyntax method) => method;
        public override SyntaxNode VisitMethodInvokeExpression(MethodInvokeExpressionSyntax methodInvokeExpression) => methodInvokeExpression;
        public override SyntaxNode VisitMethodInvokeStatement(MethodInvokeStatementSyntax methodInvokeStatement) => methodInvokeStatement;
        public override SyntaxNode VisitNamespace(NamespaceSyntax ns) => ns;
        public override SyntaxNode VisitNewExpression(NewExpressionSyntax newExpression) => newExpression;
        public override SyntaxNode VisitParameter(ParameterSyntax parameter) => parameter;
        public override SyntaxNode VisitParameterList(ParameterListSyntax parameterList) => parameterList;
        public override SyntaxNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax parenthesizedExpression) => parenthesizedExpression;
        public override SyntaxNode VisitParentTypeReference(ParentTypeReferenceSyntax parentTypeReference) => parentTypeReference;
        public override SyntaxNode VisitRangeExpression(RangeExpressionSyntax rangeExpression) => rangeExpression;
        public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax returnStatement) => returnStatement;
        public override SyntaxNode VisitSizeofExpression(SizeofExpressionSyntax sizeExpression) => sizeExpression;
        public override SyntaxNode VisitStatementBlock(StatementBlockSyntax statementBlock) => statementBlock;
        public override SyntaxNode VisitSyntaxList<J>(SeparatedSyntaxList<J> list) => list;
        public override SyntaxNode VisitTernaryExpression(TernaryExpressionSyntax ternaryExpression) => ternaryExpression;
        public override SyntaxNode VisitThisExpression(ThisExpressionSyntax thisExpression) => thisExpression;
        public override SyntaxNode VisitToken(SyntaxToken token) => null;
        public override SyntaxNode VisitTokenList(SeparatedTokenList list) => list;
        public override SyntaxNode VisitTrivia(SyntaxTrivia trivia) => null;
        public override SyntaxNode VisitType(TypeSyntax type) => type;
        public override SyntaxNode VisitTypeofExpression(TypeofExpressionSyntax typeofExpression) => typeofExpression;
        public override SyntaxNode VisitTypeReference(TypeReferenceSyntax typeReference) => typeReference;
        public override SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax unaryExpression) => unaryExpression;
        public override SyntaxNode VisitVariableAssignmentExpression(VariableAssignmentExpressionSyntax variableAssignExpression) => variableAssignExpression;
        public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax variableDeclaration) => variableDeclaration;
        public override SyntaxNode VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax variableDeclarationStatement) => variableDeclarationStatement;
        public override SyntaxNode VisitVariableReferenceExpression(VariableReferenceExpressionSyntax variableReferenceExpression) => variableReferenceExpression;
    }
}
