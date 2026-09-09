
namespace LumaSharp.Compiler.SemanticsOld
{
    public interface INamespaceReferenceSymbol : IReferenceSymbol
    {
        // Properties
        string NamespaceName { get; }

        int NamespaceDepth { get; }
        INamespaceReferenceSymbol ParentNamespace { get; }

        IReadOnlyList<INamespaceReferenceSymbol> NamespacesInScope { get; }
        IReadOnlyList<ITypeReferenceSymbol> TypesInScope { get; }
    }
}
