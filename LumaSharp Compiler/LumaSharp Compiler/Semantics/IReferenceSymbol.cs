using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics
{
    public interface IReferenceSymbol
    {
        // Properties
        ILibraryReferenceSymbol LibrarySymbol { get; }

        _TokenHandle SymbolToken { get; }
    }
}
