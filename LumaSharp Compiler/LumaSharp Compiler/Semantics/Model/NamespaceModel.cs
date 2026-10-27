using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    internal sealed class NamespaceModel : INamespaceSymbol
    {
        // Private
        private readonly SemanticModel model;
        private readonly NamespaceSyntax ns;

        // Properties
        public string FullName => throw new NotImplementedException();
        public string Name => ns.Name[ns.Name.Count - 1].Text;
        public SyntaxSpan? Source => ns.GetSpan();

        // Methods
        public INamespaceSymbol GetDeclaringNamespace()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<INamespaceSymbol> GetNamespaces()
        {
            throw new NotImplementedException();
        }

        public ITypeSymbol GetTypes()
        {
            throw new NotImplementedException();
        }
    }
}
