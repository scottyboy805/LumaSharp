using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics
{
    public interface ISymbolProvider
    {
        // Methods
        ITypeReferenceSymbol ResolveTypeSymbol(PrimitiveType primitiveType);

        ITypeReferenceSymbol ResolveTypeSymbol(TypeReferenceSyntax reference);
    }
}
