
namespace LumaSharp.Compiler.Semantics
{
    public interface IAliasIdentifierReferenceSymbol : IIdentifierReferenceSymbol
    {
        // Properties
        ITypeReferenceSymbol AliasType { get; }
    }
}
