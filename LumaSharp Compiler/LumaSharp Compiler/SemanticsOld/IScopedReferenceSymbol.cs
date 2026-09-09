
namespace LumaSharp.Compiler.SemanticsOld
{
    public interface IScopedReferenceSymbol
    {
        // Properties
        string ScopeName { get; }

        IReferenceSymbol ParentSymbol { get; }

        ILocalIdentifierReferenceSymbol[] LocalsInScope { get; }
    }
}
