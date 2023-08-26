
namespace LumaSharp_Compiler.Semantics
{
    public interface IMethodReferenceSymbol : IReferenceSymbol
    {
        // Properties
        string MethodName { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }

        ITypeReferenceSymbol ReturnTypeSymbol { get; }

        ITypeReferenceSymbol[] GenericArgumentSymbols { get; }

        ITypeReferenceSymbol[] ParameterSymbols { get; }
    }
}
