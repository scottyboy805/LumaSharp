
namespace LumaSharp_Compiler.Semantics
{
    public interface ILibraryReferenceSymbol
    {
        // Properties
        int LibraryToken { get; }

        string LibraryName { get; }
    }
}
