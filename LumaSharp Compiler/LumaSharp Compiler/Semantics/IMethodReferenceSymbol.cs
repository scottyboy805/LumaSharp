
using LumaSharp.Runtime.Handle;

namespace LumaSharp_Compiler.Semantics
{
    public interface IMethodReferenceSymbol : IIdentifierReferenceSymbol
    {
        // Properties
        string MethodName { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }

        ITypeReferenceSymbol ReturnTypeSymbol { get; }

        IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols { get; }

        ILocalIdentifierReferenceSymbol[] ParameterSymbols { get; }

        bool IsGlobal { get; }

        bool HasBody { get; }

        _MethodHandle MethodHandle { get; }
    }
}
