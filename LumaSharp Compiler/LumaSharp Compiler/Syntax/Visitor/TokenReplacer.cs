

namespace LumaSharp.Compiler.AST.Visitor
{
    internal sealed class TokenReplacer : SyntaxRewriter
    {
        // Private
        private readonly SyntaxToken match;
        private readonly SyntaxToken replacement;

        // Constructor
        public TokenReplacer(SyntaxToken match, SyntaxToken replacement)
        {
            this.match = match;
            this.replacement = replacement;
        }

        // Methods
        public override SyntaxNode VisitTypeReference(TypeReferenceSyntax typeReference)
        {
            // Check for primitive
            if (typeReference.IsPrimitiveType == true)
                return new TypeReferenceSyntax(
                    TokenExchange(typeReference.Identifier),
                    DefaultVisit(typeReference.ArrayParameters));

            // Handle complex
            return new TypeReferenceSyntax(
                DefaultVisit(typeReference.Namespace),
                typeReference.ParentTypes,
                TokenExchange(typeReference.Identifier),
                DefaultVisit(typeReference.GenericArguments),
                DefaultVisit(typeReference.ArrayParameters));
        }

        public override SyntaxNode VisitField(FieldSyntax field)
        {
            return new FieldSyntax(
                TokenExchange(field.Identifier),
                field.Attributes,
                field.AccessModifiers,
                DefaultVisit(field.FieldType),
                DefaultVisit(field.FieldAssignment));
        }


        #region Expression
        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax literalExpression)
        {
            return new LiteralExpressionSyntax(
                TokenExchange(literalExpression.Value),
                literalExpression.Descriptor != null
                    ? TokenExchange(literalExpression.Descriptor.Value)
                    : (SyntaxToken?)null);
        }
        #endregion

        private SyntaxToken TokenExchange(SyntaxToken current)
        {
            if (match.Equals(current) == true)
                return replacement;

            return current;
        }
    }
}
