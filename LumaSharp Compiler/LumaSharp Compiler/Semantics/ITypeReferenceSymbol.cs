
namespace LumaSharp_Compiler.Semantics
{
    public interface ITypeReferenceSymbol
    {
        // Properties
        ILibraryReferenceSymbol LibrarySymbol { get; }

        int TypeToken { get; }

        string TypeName { get; }
    }
}
