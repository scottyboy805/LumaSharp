
namespace LumaSharp.Compiler.AST.Visitor
{
    internal sealed class WhitespaceRewriter : SyntaxRewriter
    {
        // Private
        private static readonly SyntaxTrivia leadingWhitespace = Syntax.LeadingTrivia(SyntaxTriviaKind.Whitespace, " ");
        private static readonly SyntaxTrivia trailingWhitespace = Syntax.TrailingTrivia(SyntaxTriviaKind.Whitespace, " ");

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

        public override SyntaxNode VisitVariableAssignmentExpression(VariableAssignmentExpressionSyntax variableAssignExpression)
        {
            return new VariableAssignmentExpressionSyntax(
                variableAssignExpression.Assign.WithTrivia(new[] { leadingWhitespace, trailingWhitespace }),
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
                e => new SeparatedSyntaxList<J>.SyntaxSeparatedElement(e.Syntax, e.Separator != null
                    ? e.Separator.Value.WithTrivia(trailingWhitespace)
                    : null)));
        }
        #endregion

        public override SyntaxNode VisitField(FieldSyntax field)
        {
            return new FieldSyntax(
                field.Identifier.WithTrivia(leadingWhitespace),
                field.Attributes,
                field.AccessModifiers,
                DefaultVisit(field.FieldType),
                DefaultVisit(field.FieldAssignment));
        }

        public override SyntaxNode VisitMethod(MethodSyntax method)
        {
            return new MethodSyntax(
                method.Identifier.WithTrivia(leadingWhitespace),
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
                returnStatement.Keyword.WithTrivia(trailingWhitespace),
                DefaultVisit(returnStatement.Expressions),
                returnStatement.Semicolon);
        }


        #region Expression
        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax binaryExpression)
        {
            return new BinaryExpressionSyntax(
                DefaultVisit(binaryExpression.Left),
                binaryExpression.Operation.WithTrivia(new[] { leadingWhitespace, trailingWhitespace }),
                DefaultVisit(binaryExpression.Right));
        }

        public override SyntaxNode VisitNewExpression(NewExpressionSyntax newExpression)
        {
            return new NewExpressionSyntax(
                newExpression.Keyword.WithTrivia(trailingWhitespace),
                DefaultVisit(newExpression.NewType),
                DefaultVisit(newExpression.ArgumentList));
        }

        public override SyntaxNode VisitTernaryExpression(TernaryExpressionSyntax ternaryExpression)
        {
            return new TernaryExpressionSyntax(
                DefaultVisit(ternaryExpression.Condition).WithTrivia(trailingWhitespace),
                ternaryExpression.Ternary.WithTrivia(trailingWhitespace),
                DefaultVisit(ternaryExpression.TrueExpression).WithTrivia(trailingWhitespace),
                ternaryExpression.Colon.WithTrivia(trailingWhitespace),
                DefaultVisit(ternaryExpression.FalseExpression));
        }
        #endregion
    }
}
