
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Reference
{
    internal sealed class ReferenceLibrary : ILibraryReferenceSymbol
    {
        // Type
        //private class NamedTypeCollection
        //{
        //    // Public
        //    public string namespaceName = null;
        //    public List<ITypeReferenceSymbol> types = new List<ITypeReferenceSymbol>();
        //    public List<NamedTypeCollection> namedTypes = new List<NamedTypeCollection>();
        //}

        // Private
        private string libraryName = null;
        private _TokenHandle libraryToken = default;
        private List<ITypeReferenceSymbol> rootTypes = new List<ITypeReferenceSymbol>();
        private List<INamespaceReferenceSymbol> namedTypes = new List<INamespaceReferenceSymbol>();
        //private List<NamedTypeCollection> namedTypes = new List<NamedTypeCollection>();

        // Properties
        public string LibraryName
        {
            get { return libraryName; }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return this; }
        }

        public _TokenHandle SymbolToken
        {
            get { return libraryToken; }
        }

        public IReadOnlyList<ITypeReferenceSymbol> DeclaredTypes
        {
            get { return rootTypes; }
        }

        public IReadOnlyList<INamespaceReferenceSymbol> DeclaredNamedTypes
        {
            get { return namedTypes; }
        }

        // Constructor
        public ReferenceLibrary(string libraryName, _TokenHandle libraryToken = default)
        {
            this.libraryName = libraryName;
            this.libraryToken = libraryToken;
        }

        // Methods
        public void DeclareType(ITypeReferenceSymbol type)
        {
            if(type != null && rootTypes.Contains(type) == false)
                rootTypes.Add(type);
        }

        public void DeclareNamedType(ITypeReferenceSymbol namedType)
        {
            //// Get the namespace
            //string[] namespaceIdentifiers = namedType.NamespaceName;

            //// Check for invalid
            //if (namespaceIdentifiers == null)
            //    throw new InvalidOperationException("Cannot declare a names type which does not have a namespace");

            //// Get the target namespace
            //NamespaceModel declaringNamespace = null;

            //for(int i = 0; i < namedType.NamespaceName.Length; i++)
            //{
            //    // Move down the hierarchy chain
            //    declaringNamespace = GetOrCreateNamespace(namedType.NamespaceName[i], i, declaringNamespace);
            //}

            // Check for invalid
            if (namedType.NamespaceName == null)
                throw new InvalidOperationException("Cannot declare a names type which does not have a namespace");

            // Declare the namespace
            NamespaceModel declaringNamespace = DeclareNamespace(namedType.NamespaceName);

            // Declare the type
            declaringNamespace.AddType(namedType);
        }

        public NamespaceModel DeclareNamespace(SeparatedTokenList namespaceName)
        {
            // Get the namespace
            string[] namespaceIdentifiers = namespaceName.Select(i => i.Text).ToArray();

            // Declare with identifiers
            return DeclareNamespace(namespaceIdentifiers);
        }

        private NamespaceModel DeclareNamespace(string[] namespaceIdentifiers)
        {
            // Get the target namespace
            NamespaceModel declaringNamespace = null;

            for (int i = 0; i < namespaceIdentifiers.Length; i++)
            {
                // Move down the hierarchy chain
                declaringNamespace = GetOrCreateNamespace(namespaceIdentifiers[i], i, declaringNamespace);
            }
            return declaringNamespace;
        }

        private NamespaceModel GetOrCreateNamespace(string name, int depth, NamespaceModel parent)
        {
            // Try to find
            NamespaceModel match = null;
            
            if(parent != null)
            {
                // Search parent namespace
                if (parent.NamespacesInScope != null)
                    match = parent.NamespacesInScope.FirstOrDefault(n => n.NamespaceName == name) as NamespaceModel;
            }
            else
            {
                // Search root namespaces
                match = namedTypes.FirstOrDefault(n => n.NamespaceName == name) as NamespaceModel;
            }

            // Check for found
            if(match == null)
            {
                match = new NamespaceModel(name, depth, parent);

                // Register as child
                if (parent != null)
                {
                    parent.AddNamespace(match);
                }
                // Regsiter as root
                else
                {
                    namedTypes.Add(match);
                }
            }

            return match as NamespaceModel;
        }

        //public void DeclareType(string[] namespaceName, ITypeReferenceSymbol type)
        //{
        //    // Check for root type
        //    if(namespaceName == null || namespaceName.Length == 0)
        //    {
        //        // Add root
        //        if(rootTypes.Contains(type) == false)
        //            rootTypes.Add(type);
        //    }
        //    else
        //    {
        //        NamedTypeCollection current = null;

        //        for(int i = 0 ; i < rootTypes.Count; i++)
        //        {
        //            // Check for no current
        //            if (current == null)
        //            {
        //                // Try to find
        //                current = namedTypes.Find(n => n.namespaceName == namespaceName[i]);

        //                // Create new
        //                if(current == null)
        //                {
        //                    // Create namespace
        //                    current = new NamedTypeCollection();
        //                    current.namespaceName = namespaceName[i];

        //                    // Add named type
        //                    namedTypes.Add(current);
        //                }
        //            }
        //            else
        //            {
        //                // Try to find
        //                NamedTypeCollection parent = current;
        //                current = current.namedTypes.Find(n => n.namespaceName == namespaceName[i]);

        //                // Create new
        //                if (current == null)
        //                {
        //                    // Create namespace
        //                    current = new NamedTypeCollection();
        //                    current.namespaceName = namespaceName[i];

        //                    // Add named type
        //                    parent.namedTypes.Add(current);
        //                }
        //            }

        //            // Add type
        //            current.types.Add(type);
        //        }
        //    }
        //}

        public IReadOnlyList<ITypeReferenceSymbol> GetDeclaredRootTypes()
        {
            return rootTypes;
        }

        //public IReadOnlyList<ITypeReferenceSymbol> GetDeclaredNamedTypes(string[] namespaceIdentifiers)
        //{
        //    NamedTypeCollection namedTypeCollection = null;

        //    // Process all namespace identifiers in sequence
        //    for (int i = 0; i < namespaceIdentifiers.Length; i++)
        //    {
        //        // Try to match
        //        if(namedTypeCollection == null)
        //        {
        //            // Try to find matched namespace
        //            namedTypeCollection = namedTypes.Find(n => n.namespaceName == namespaceIdentifiers[i]);
        //        }
        //        else
        //        {
        //            // Try to find nested matched namespace
        //            namedTypeCollection = namedTypeCollection.namedTypes.Find(n => n.namespaceName == namespaceIdentifiers[i]);
        //        }

        //        // Check for not resolved
        //        if (namedTypeCollection == null)
        //            return null;
        //    }

        //    // Check for found
        //    if (namedTypeCollection != null)
        //        return namedTypeCollection.types;

        //    // No matched types declared
        //    return null;
        //}
    }
}
