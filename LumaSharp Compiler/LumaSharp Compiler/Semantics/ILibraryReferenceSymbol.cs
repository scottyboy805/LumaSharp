namespace LumaSharp.Compiler.Semantics
{
    public interface ILibraryReferenceSymbol : IReferenceSymbol
    {
        // Properties
        string LibraryName { get; }
    }
}
