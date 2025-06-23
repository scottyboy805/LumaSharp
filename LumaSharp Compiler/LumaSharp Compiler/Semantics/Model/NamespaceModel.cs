using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class NamespaceModel : SymbolModel, INamespaceReferenceSymbol
    {
        // Private
        private readonly StringModel namespaceName;
        private readonly List<NamespaceModel> namespaceMembers = new();
        private readonly List<TypeModel> typeMembers = new();

        // Properties
        public string NamespaceName
        {
            get { return namespaceName.Text; }
        }

        public int NamespaceDepth
        {
            get
            {
                int depth = 1;
                NamespaceModel current = this.Parent as NamespaceModel;

                // move up the hierarchy searching for parent namespaces
                while(current != null)
                {
                    depth++;
                    current = current.Parent as NamespaceModel;
                }
                return depth;
            }
        }

        public INamespaceReferenceSymbol ParentNamespace
        {
            get { return Parent as INamespaceReferenceSymbol; }
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
            get { return Model.LibrarySymbol; }
        }

        public _TokenHandle Token
        {
            get { return default; }
        }

        // Constructor
        public NamespaceModel(SyntaxToken namespaceName, NamespaceModel parent)
            : base(namespaceName.Span)
        {
            this.namespaceName = new StringModel(namespaceName);
            this.parent = parent;
        }

        public void AddType(TypeModel typeModel)
        {
            // Check for null
            if(typeModel == null)
                throw new ArgumentNullException(nameof(typeModel));

            // Add member
            typeMembers.Add(typeModel);
        }

        public void AddNamespace(NamespaceModel namespaceModel)
        {
            // Check for null
            if(namespaceModel == null)
                throw new ArgumentNullException(nameof(namespaceModel));

            // Add member
            namespaceMembers.Add(namespaceModel);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve name
            namespaceName.ResolveSymbols(provider, report);

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
