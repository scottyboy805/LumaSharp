
namespace LumaSharp_Compiler.Semantics
{
    public interface IMethodReferenceSymbol : IReferenceSymbol
    {
        // Properties
        string MethodName { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }

        ITypeReferenceSymbol ReturnTypeSymbol { get; }

        IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols { get; }

        ILocalIdentifierReferenceSymbol[] ParameterSymbols { get; }

        bool HasBody { get; }
    }
}
