using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics
{
    public sealed class TypeReferenceModel
    {
        // Private
        private TypeReferenceSyntax syntax = null;
        private ITypeReferenceSymbol resolvedType = null;

        // Properties
        public ITypeReferenceSymbol ResolvedType
        {
            get { return resolvedType; }
        }

        // Constructor
        internal TypeReferenceModel(TypeReferenceSyntax syntax)
        {
            this.syntax = syntax;
        }

        // Methods
        public bool Resolve(object context)
        {
            return false;
        }
    }
}
