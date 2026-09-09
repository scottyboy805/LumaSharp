
namespace LumaSharp.Compiler.SemanticsOld
{
    public interface IMethodReferenceSymbol : IMemberReferenceSymbol
    {
        // Properties
        ITypeReferenceSymbol[] ReturnTypeSymbols { get; }

        IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols { get; }

        ILocalIdentifierReferenceSymbol[] ParameterSymbols { get; }

        bool IsOverride { get; }

        bool HasBody { get; }
    }
}
