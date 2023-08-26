namespace LumaSharp_Compiler.Semantics
{
    public interface ILibraryReferenceSymbol : IReferenceSymbol
    {
        // Properties
        string LibraryName { get; }
    }
}
