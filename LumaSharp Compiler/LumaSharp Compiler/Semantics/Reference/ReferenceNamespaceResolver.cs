using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Semantics.Reference
{
    internal class ReferenceNamespaceResolver
    {
        // Constructor
        public ReferenceNamespaceResolver() { }

        // Methods
        public bool ResolveReferenceNamespaceSymbol(ReferenceLibrary thisLibrary, ReferenceLibrary[] referenceLibraries, NamespaceName namespaceName, out INamespaceReferenceSymbol resovledNamespace)
        {
            // Check this library first
            if (ResolveReferenceNamespaceSymbol(thisLibrary, namespaceName, out resovledNamespace) == true)
                return true;

            // Check references libraries in order
            for(int i = 0; i < referenceLibraries.Length; i++)
            {
                // Check for found
                if (ResolveReferenceNamespaceSymbol(referenceLibraries[i], namespaceName, out resovledNamespace) == true)
                    return true;
            }

            // Failed to resolve
            resovledNamespace = null;
            return false;
        }

        public bool ResolveReferenceNamespaceSymbol(ReferenceLibrary library, NamespaceName namespaceName, out INamespaceReferenceSymbol resolvedNamespace)
        {
            INamespaceReferenceSymbol current = null;

            // Check all identifiers
            for(int i = 0; i < namespaceName.Identifiers.Length; i++)
            {
                // Select best fitting namespaces
                IEnumerable<INamespaceReferenceSymbol> namespaceSymbols = current == null
                    ? library.DeclaredNamedTypes
                    : current.NamespacesInScope;

                bool match = false;

                // Check all available namespaces
                foreach (INamespaceReferenceSymbol namespaceSymbol in namespaceSymbols)
                {
                    // Check for matching name
                    if (namespaceName.Identifiers[i].Text == namespaceSymbol.NamespaceName)
                    {
                        match = true;
                        current = namespaceSymbol;
                        break;
                    }
                }

                // Check for all matched
                if(match == true && i == namespaceName.Identifiers.Length - 1)
                {
                    resolvedNamespace = current;
                    return true;
                }
            }

            //// Check all namespaces
            //foreach (INamespaceReferenceSymbol nsSymbol in library.DeclaredNamedTypes)
            //{
            //    // Check for matching depth
            //    if (nsSymbol.NamespaceDepth == namespaceName.Identifiers.Length)
            //    {
            //        bool allMatched = true;

            //        // Compare identifiers in sequence
            //        for (int i = 0; i < nsSymbol.NamespaceDepth; i++)
            //        {
            //            if (nsSymbol.Namespace[i] != namespaceName.Identifiers[i].Text)
            //            {
            //                allMatched = false;
            //                break;
            //            }
            //        }

            //        // Check for matched
            //        if (allMatched == true)
            //        {
            //            resolvedNamespace = nsSymbol;
            //            return true;
            //        }
            //    }
            //}

            // Failed to resolve
            resolvedNamespace = null;
            return false;
        }
    }
}
