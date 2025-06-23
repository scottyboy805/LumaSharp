
namespace LumaSharp.Compiler.Semantics
{
    public interface IFieldReferenceSymbol : IIdentifierReferenceSymbol
    {
        // Properties
        bool IsGlobal { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }
    }
}
