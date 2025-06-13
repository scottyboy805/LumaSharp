
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
            // Check for trivia
            if (list == null)
                return null;

            // Check for any
            if (list.Count == 0)
                return list;

            // Create the new list with whitespace separators
            return new SeparatedSyntaxList<J>(list.SeparatorKind, list.Elements.Select(
                e => new SeparatedSyntaxList<J>.SyntaxSeparatedElement(DefaultVisit(e.Syntax), e.Separator != null
                    ? e.Separator.Value.WithTrailingTrivia(whitespace)
                    : null)));
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

        public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax returnStatement)
        {
            return new ReturnStatementSyntax(
                returnStatement.Keyword.WithTrailingTrivia(whitespace),
                DefaultVisit(returnStatement.Expressions),
                returnStatement.Semicolon);
        }

        #region Statement
        public override SyntaxNode VisitStatementBlock(StatementBlockSyntax statementBlock)
        {
            return new StatementBlockSyntax(
                statementBlock.LBlock.WithLeadingTrivia(newLine),
                statementBlock.Statements,
                statementBlock.RBlock.WithLeadingTrivia(newLine));
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
