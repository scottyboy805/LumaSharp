
namespace LumaSharp_Compiler.Semantics.Reference
{
    internal sealed class ReferenceLibrary : ILibraryReferenceSymbol
    {
        // Type
        private class NamedTypeCollection
        {
            // Public
            public string namespaceName = null;
            public List<ITypeReferenceSymbol> types = new List<ITypeReferenceSymbol>();
            public List<NamedTypeCollection> namedTypes = new List<NamedTypeCollection>();
        }

        // Private
        private string libraryName = null;
        private int libraryToken = -1;
        private List<ITypeReferenceSymbol> rootTypes = new List<ITypeReferenceSymbol>();
        private List<NamedTypeCollection> namedTypes = new List<NamedTypeCollection>();

        // Properties
        public string LibraryName
        {
            get { return libraryName; }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return this; }
        }

        public int SymbolToken
        {
            get { return libraryToken; }
        }

        // Constructor
        public ReferenceLibrary(string libraryName, int libraryToken = -1)
        {
            this.libraryName = libraryName;
            this.libraryToken = libraryToken;
        }

        // Methods
        public void DeclareType(string[] namespaceName, ITypeReferenceSymbol type)
        {
            // Check for root type
            if(namespaceName == null || namespaceName.Length == 0)
            {
                // Add root
                if(rootTypes.Contains(type) == false)
                    rootTypes.Add(type);
            }
            else
            {
                NamedTypeCollection current = null;

                for(int i = 0 ; i < rootTypes.Count; i++)
                {
                    // Check for no current
                    if (current == null)
                    {
                        // Try to find
                        current = namedTypes.Find(n => n.namespaceName == namespaceName[i]);

                        // Create new
                        if(current == null)
                        {
                            // Create namespace
                            current = new NamedTypeCollection();
                            current.namespaceName = namespaceName[i];

                            // Add named type
                            namedTypes.Add(current);
                        }
                    }
                    else
                    {
                        // Try to find
                        NamedTypeCollection parent = current;
                        current = current.namedTypes.Find(n => n.namespaceName == namespaceName[i]);

                        // Create new
                        if (current == null)
                        {
                            // Create namespace
                            current = new NamedTypeCollection();
                            current.namespaceName = namespaceName[i];

                            // Add named type
                            parent.namedTypes.Add(current);
                        }
                    }

                    // Add type
                    current.types.Add(type);
                }
            }
        }
    }
}
