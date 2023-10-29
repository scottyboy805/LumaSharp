using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using System.Linq;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class NamespaceModel : INamespaceReferenceSymbol
    {
        // Private
        private string name = "";
        private int depth = 0;
        private INamespaceReferenceSymbol parentNamespace = null;
        private List<INamespaceReferenceSymbol> namespaceMembers = null;
        private List<ITypeReferenceSymbol> typeMembers = null;

        // Properties
        public string NamespaceName
        {
            get { return name; }
        }

        public int NamespaceDepth
        {
            get { return depth; }
        }

        public INamespaceReferenceSymbol ParentNamespace
        {
            get { return parentNamespace; }
        }

        public IReadOnlyList<INamespaceReferenceSymbol> NamespacesInScope
        {
            get { return namespaceMembers; }
        }

        public IReadOnlyList<ITypeReferenceSymbol> TypesInScope
        {
            get { return typeMembers; }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return null; }
        }

        public int SymbolToken
        {
            get { return -1; }
        }

        // Constructor
        public NamespaceModel(string name, int depth = 0, INamespaceReferenceSymbol parentNamespace = null)
        {
            this.name = name;
            this.depth = depth;
            this.parentNamespace = parentNamespace;

            //// Build types
            //List<TypeModel> types = new List<TypeModel>();

            //// Add all types
            //types.AddRange(syntax.DescendantsOfType<TypeSyntax>().Select(t => new TypeModel(model, this, t)));

            //// Add all contracts
            //types.AddRange(syntax.DescendantsOfType<ContractSyntax>().Select(c => new TypeModel(model, this, c)));

            //// Add all enums
            //types.AddRange(syntax.DescendantsOfType<EnumSyntax>().Select(e => new TypeModel(model, this, e)));

            //this.typeMembers = types.ToArray();
        }

        public void AddType(ITypeReferenceSymbol typeSymbol)
        {
            // Check for null
            if(typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            // Create collection
            if (typeMembers == null)
                typeMembers = new List<ITypeReferenceSymbol>();

            // Add member
            typeMembers.Add(typeSymbol);
        }

        public void AddNamespace(INamespaceReferenceSymbol namespaceSymbol)
        {
            // Check for null
            if(namespaceSymbol == null)
                throw new ArgumentNullException(nameof(namespaceSymbol));

            // Create collection
            if (namespaceMembers == null)
                namespaceMembers = new List<INamespaceReferenceSymbol>();

            // Add member
            namespaceMembers.Add(namespaceSymbol);
        }

        public void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve all types
            foreach(TypeModel typeModel in typeMembers)
            {
                typeModel.ResolveSymbols(provider, report);
            }

            // Resolve all child namespaces
            foreach(NamespaceModel namespaceModel in namespaceMembers)
            {
                namespaceModel.ResolveSymbols(provider, report);
            }
        }
    }
}
