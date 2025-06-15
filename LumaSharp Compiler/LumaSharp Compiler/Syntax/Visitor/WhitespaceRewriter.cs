
namespace LumaSharp.Compiler.AST.Visitor
{
    internal sealed class WhitespaceRewriter : SyntaxRewriter
    {
        // Private
        private static readonly SyntaxTrivia whitespace = Syntax.Trivia(SyntaxTriviaKind.Whitespace, " ");
        private static readonly SyntaxTrivia newLine = Syntax.Trivia(SyntaxTriviaKind.Newline, "\n");

        // Methods
        public override SyntaxNode VisitTypeReference(TypeReferenceSyntax typeReference)
        {
            // Check for primitive
            if (typeReference.IsPrimitiveType == true)
                return new TypeReferenceSyntax(
                    typeReference.Identifier,
                    DefaultVisit(typeReference.ArrayParameters));

            return new TypeReferenceSyntax(
                typeReference.Namespace,
                typeReference.ParentTypes,
                typeReference.Identifier,
                DefaultVisit(typeReference.GenericArguments),
                DefaultVisit(typeReference.ArrayParameters));
        }

        public override SyntaxNode VisitAttribute(AttributeSyntax attribute)
        {
            return new AttributeSyntax(
                attribute.Hash,
                DefaultVisit(attribute.AttributeType),
                DefaultVisit(attribute.ArgumentList))
                .WithTrailingTrivia(whitespace);
        }

        public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax variableDeclaration)
        {
            return new VariableDeclarationSyntax(
                DefaultVisit(variableDeclaration.VariableType)?.WithTrailingTrivia(whitespace),
                DefaultVisit(variableDeclaration.Identifiers),
                DefaultVisit(variableDeclaration.Assignment));
        }

        public override SyntaxNode VisitVariableAssignmentExpression(VariableAssignmentExpressionSyntax variableAssignExpression)
        {
            return new VariableAssignmentExpressionSyntax(
                variableAssignExpression.Assign.WithLeadingTrivia(whitespace).WithTrailingTrivia(whitespace),
                DefaultVisit(variableAssignExpression.AssignExpressions));
        }

        #region List
        public override SyntaxNode VisitArgumentList(ArgumentListSyntax argumentList)
        {
            return new ArgumentListSyntax(
                argumentList.LParen,
                VisitSyntaxList(argumentList) as SeparatedSyntaxList<ExpressionSyntax>,
                argumentList.RParen);
        }

        public override SyntaxNode VisitSyntaxList<J>(SeparatedSyntaxList<J> list)
        {
            return VisitSyntaxList(list, false);
        }

        private SeparatedSyntaxList<J> VisitSyntaxList<J>(SeparatedSyntaxList<J> list, bool leadingSeparatorSpace) where J : SyntaxNode
        {
            // Check for null
            if (list == null)
                return null;

            // Check for any
            if (list.Count == 0)
                return list;

            // Create the new list with whitespace separators
            return new SeparatedSyntaxList<J>(list.SeparatorKind, list.Elements.Select(
                e => new SeparatedSyntaxList<J>.SyntaxSeparatedElement(DefaultVisit(e.Syntax), e.Separator != null
                    ? leadingSeparatorSpace == true
                        ? e.Separator.Value.WithLeadingTrivia(whitespace).WithTrailingTrivia(whitespace)
                        : e.Separator.Value.WithTrailingTrivia(whitespace)
                    : (SyntaxToken?)null)));
        }

        public override SyntaxNode VisitTokenList(SeparatedTokenList list)
        {
            // Check for null
            if (list == null)
                return null;

            // Check for any
            if (list.Count == 0)
                return list;

            // Create the new list with whitespace separators
            return new SeparatedTokenList(list.SeparatorKind, list.Elements.Select(
                e => new SeparatedTokenList.TokenSeparatedElement(e.Token, e.Separator != null
                    ? e.Separator.Value.WithTrailingTrivia(whitespace)
                    : (SyntaxToken?)null)), list.TokenKind);
        }

        public override SyntaxNode VisitBaseTypeList(BaseTypeListSyntax baseTypeList)
        {
            return new BaseTypeListSyntax(
                baseTypeList.Colon.WithLeadingTrivia(whitespace).WithTrailingTrivia(whitespace),
                VisitSyntaxList(baseTypeList, false));
        }

        public override SyntaxNode VisitGenericParameterList(GenericParameterListSyntax genericParameterList)
        {
            return new GenericParameterListSyntax(
                genericParameterList.LGeneric,
                VisitSyntaxList(genericParameterList) as SeparatedSyntaxList<GenericParameterSyntax>,
                genericParameterList.RGeneric);
        }

        public override SyntaxNode VisitGenericParameter(GenericParameterSyntax genericParameter)
        {
            return new GenericParameterSyntax(
                genericParameter.Identifier,
                DefaultVisit(genericParameter.Constraints));
        }

        public override SyntaxNode VisitGenericParameterConstraints(GenericParameterConstraintsSyntax genericConstraints)
        {
            return new GenericParameterConstraintsSyntax(
                genericConstraints.Colon.WithLeadingTrivia(whitespace).WithTrailingTrivia(whitespace),
                VisitSyntaxList(genericConstraints.Constraints, true));
        }

        public override SyntaxNode VisitParameterList(ParameterListSyntax parameterList)
        {
            return new ParameterListSyntax(
                parameterList.LParen,
                VisitSyntaxList(parameterList) as SeparatedSyntaxList<ParameterSyntax>,
                parameterList.RParen);
        }

        public override SyntaxNode VisitParameter(ParameterSyntax parameter)
        {
            return new ParameterSyntax(
                parameter.Attributes?.Select(p => DefaultVisit(p)).ToArray(),
                DefaultVisit(parameter.ParameterType),
                parameter.Identifier.WithLeadingTrivia(whitespace),
                DefaultVisit(parameter.Assignment),
                parameter.Enumerable?.WithLeadingTrivia(whitespace));
        }
        #endregion

        #region Member
        public override SyntaxNode VisitContract(ContractSyntax contract)
        {
            return new ContractSyntax(
                contract.Keyword.WithTrailingTrivia(whitespace),
                contract.Identifier,
                contract.Attributes?.Select(a => DefaultVisit(a)).ToArray(),
                contract.AccessModifiers?.Select(m => m.WithTrailingTrivia(whitespace)).ToArray(),
                DefaultVisit(contract.GenericParameters),
                DefaultVisit(contract.BaseTypes),
                DefaultVisit(contract.Members));
        }

        public override SyntaxNode VisitEnum(EnumSyntax e)
        {
            return new EnumSyntax(
                e.Keyword.WithTrailingTrivia(whitespace),
                e.Identifier,
                e.Attributes?.Select(a => DefaultVisit(a)).ToArray(),
                e.AccessModifiers?.Select(m => m.WithTrailingTrivia(whitespace)).ToArray(),
                DefaultVisit(e.UnderlyingType),
                DefaultVisit(e.Body));
        }

        public override SyntaxNode VisitEnumBlock(EnumBlockSyntax enumBlock)
        {
            return new EnumBlockSyntax(
                enumBlock.LBlock.WithLeadingTrivia(newLine),
                VisitSyntaxList(enumBlock, false),
                enumBlock.RBlock.WithLeadingTrivia(newLine));
        }

        public override SyntaxNode VisitField(FieldSyntax field)
        {
            return new FieldSyntax(
                field.Identifier.WithLeadingTrivia(whitespace),
                field.Attributes,
                field.AccessModifiers,
                DefaultVisit(field.FieldType),
                DefaultVisit(field.FieldAssignment));
        }

        public override SyntaxNode VisitMethod(MethodSyntax method)
        {
            return new MethodSyntax(
                method.Identifier.WithLeadingTrivia(whitespace),
                method.Attributes,
                method.AccessModifiers,
                DefaultVisit(method.ReturnTypes),
                DefaultVisit(method.GenericParameters),
                DefaultVisit(method.Parameters),
                method.Override,
                DefaultVisit(method.Body),
                DefaultVisit(method.Lambda));
        }

        public override SyntaxNode VisitType(TypeSyntax type)
        {
            return new TypeSyntax(
                type.Keyword.WithTrailingTrivia(whitespace),
                type.Identifier,
                type.Attributes?.Select(a => DefaultVisit(a)).ToArray(),
                type.AccessModifiers?.Select(m => m.WithTrailingTrivia(whitespace)).ToArray(),
                DefaultVisit(type.GenericParameters),
                type.Override != null
                    ? type.Override.Value.WithLeadingTrivia(whitespace)
                    : (SyntaxToken?)null,
                DefaultVisit(type.BaseTypes),
                DefaultVisit(type.Members));
        }

        public override SyntaxNode VisitMemberBlock(MemberBlockSyntax memberBlock)
        {
            return new MemberBlockSyntax(
                memberBlock.LBlock.WithLeadingTrivia(newLine),
                memberBlock.Members?.Select(m => DefaultVisit(m)),
                memberBlock.RBlock.WithLeadingTrivia(newLine));
        }
        #endregion        

        #region Statement
        public override SyntaxNode VisitForStatement(ForStatementSyntax forStatement)
        {
            return new ForStatementSyntax(
                forStatement.Keyword.WithTrailingTrivia(whitespace),
                DefaultVisit(forStatement.Variable),
                forStatement.VariableSemicolon,
                DefaultVisit(forStatement.Condition)?.WithLeadingTrivia(whitespace),
                forStatement.ConditionSemicolon,
                DefaultVisit(forStatement.Increments)?.WithLeadingTrivia(whitespace),
                DefaultVisit(forStatement.Statement));
        }

        public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax returnStatement)
        {
            return new ReturnStatementSyntax(
                returnStatement.Keyword.WithTrailingTrivia(whitespace),
                DefaultVisit(returnStatement.Expressions),
                returnStatement.Semicolon);
        }

        public override SyntaxNode VisitStatementBlock(StatementBlockSyntax statementBlock)
        {
            return new StatementBlockSyntax(
                statementBlock.LBlock.WithLeadingTrivia(newLine),
                statementBlock.Statements,
                statementBlock.RBlock.WithLeadingTrivia(newLine));
        }

        public override SyntaxNode VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax variableDeclarationStatement)
        {
            return new VariableDeclarationStatementSyntax(
                DefaultVisit(variableDeclarationStatement.Variable),
                variableDeclarationStatement.Semicolon);
        }
        #endregion


        #region Expression
        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax binaryExpression)
        {
            return new BinaryExpressionSyntax(
                DefaultVisit(binaryExpression.Left),
                binaryExpression.Operation.WithLeadingTrivia(whitespace).WithTrailingTrivia(whitespace),
                DefaultVisit(binaryExpression.Right));
        }

        public override SyntaxNode VisitNewExpression(NewExpressionSyntax newExpression)
        {
            return new NewExpressionSyntax(
                newExpression.Keyword.WithTrailingTrivia(whitespace),
                DefaultVisit(newExpression.NewType),
                DefaultVisit(newExpression.ArgumentList));
        }

        public override SyntaxNode VisitTernaryExpression(TernaryExpressionSyntax ternaryExpression)
        {
            return new TernaryExpressionSyntax(
                DefaultVisit(ternaryExpression.Condition).WithTrailingTrivia(whitespace),
                ternaryExpression.Ternary.WithTrailingTrivia(whitespace),
                DefaultVisit(ternaryExpression.TrueExpression).WithTrailingTrivia(whitespace),
                ternaryExpression.Colon.WithTrailingTrivia(whitespace),
                DefaultVisit(ternaryExpression.FalseExpression));
        }
        #endregion
    }
}
