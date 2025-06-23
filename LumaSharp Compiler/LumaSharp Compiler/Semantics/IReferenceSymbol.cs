using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics
{
    public interface IReferenceSymbol
    {
        // Properties
        ILibraryReferenceSymbol LibrarySymbol { get; }

        _TokenHandle Token { get; }
    }
}
