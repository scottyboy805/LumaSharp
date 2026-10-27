
namespace LumaSharp.Compiler.Semantics
{
    public interface INamespaceSymbol : ISymbol
    {
        // Properties
        string FullName { get; }

        // Methods
        public INamespaceSymbol GetDeclaringNamespace();

        public IReadOnlyList<INamespaceSymbol> GetNamespaces();
        public ITypeSymbol GetTypes();
    }
}
