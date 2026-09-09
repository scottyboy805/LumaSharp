
namespace LumaSharp.Compiler.Semantics.Symbol
{
    public interface IArrayTypeSymbol : ITypeSymbol
    {
        // Properties
        int ArrayRank { get; }

        // Methods
        ITypeSymbol GetElementType();
    }
}
