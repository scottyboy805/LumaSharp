
namespace LumaSharp.Compiler.SemanticsOld
{
    public interface IGenericParameterIdentifierReferenceSymbol : ITypeReferenceSymbol
    {
        bool IsTypeParameter { get; }

        bool IsMethodParameter { get; }

        ITypeReferenceSymbol[] TypeConstraintSymbols { get; }
    }
}
