
namespace LumaSharp.Compiler.SemanticsOld
{
    public interface IAliasIdentifierReferenceSymbol : IIdentifierReferenceSymbol
    {
        // Properties
        ITypeReferenceSymbol AliasType { get; }
    }
}
