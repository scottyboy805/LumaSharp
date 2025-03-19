
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics
{
    public interface IMethodReferenceSymbol : IIdentifierReferenceSymbol
    {
        // Properties
        string MethodName { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }

        ITypeReferenceSymbol[] ReturnTypeSymbols { get; }

        IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols { get; }

        ILocalIdentifierReferenceSymbol[] ParameterSymbols { get; }

        bool IsGlobal { get; }

        bool IsOverride { get; }

        bool HasBody { get; }
    }
}
