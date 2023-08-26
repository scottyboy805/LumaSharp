
namespace LumaSharp_Compiler.Semantics
{
    public interface IReferenceSymbol
    {
        // Properties
        ILibraryReferenceSymbol LibrarySymbol { get; }

        int SymbolToken { get; }
    }
}
