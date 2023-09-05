
namespace LumaSharp_Compiler.Semantics
{
    public interface IGenericParameterIdentifierReferenceSymbol : IIdentifierReferenceSymbol
    {
        bool IsTypeParameter { get; }

        bool IsMethodParameter { get; }

        int Index { get; }

        ITypeReferenceSymbol[] TypeConstraintSymbols { get; }
    }
}
