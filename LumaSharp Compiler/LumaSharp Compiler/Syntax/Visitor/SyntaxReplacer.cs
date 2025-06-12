
namespace LumaSharp.Compiler.AST.Visitor
{
    internal sealed class SyntaxReplacer<T> : SyntaxRewriter where T : SyntaxNode
    {
        // Private
        private readonly T match;
        private readonly T replacement;

        // Constructor
        public SyntaxReplacer(T match, T replacement)
        {
            this.match = match;
            this.replacement = replacement;
        }

        // Methods
        public override SyntaxNode VisitTypeReference(TypeReferenceSyntax typeReference)
        {
            return new TypeReferenceSyntax(
                SyntaxExchange(typeReference.Namespace),
                SyntaxExchange(typeReference.ParentTypes),
                typeReference.Identifier,
                SyntaxExchange(typeReference.GenericArguments),
                SyntaxExchange(typeReference.ArrayParameters));
        }

        #region Expression

        #endregion

        private J[] SyntaxExchange<J>(J[] current) where J : SyntaxNode
        {
            int index = Array.IndexOf(current, match);

            // Check for found
            if (index != -1)
            {
                // Create clone
                J[] clone = (J[])current.Clone();

                // Update element
                clone[index] = replacement as J;
                return clone;
            }

            return current;
        }

        private J SyntaxExchange<J>(J current) where J : SyntaxNode
        {
            if (match == current)
                return replacement as J;

            return current;
        }
    }
}
