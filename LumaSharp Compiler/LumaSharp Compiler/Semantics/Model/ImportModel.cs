using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class ImportModel : SymbolModel, IAliasIdentifierReferenceSymbol, INamespaceReferenceSymbol
    {
        // Private
        private ImportSyntax syntax = null;
        private INamespaceReferenceSymbol namespaceSymbol = null;
        private ITypeReferenceSymbol aliasType = null;

        // Properties
        public string NamespaceName
        {
            get
            {
                // Check for type
                if (namespaceSymbol != null)
                    return namespaceSymbol.NamespaceName;

                return null;
            }
        }

        public int NamespaceDepth
        {
            get
            {
                // Check for symbol
                if(namespaceSymbol != null)
                    return namespaceSymbol.NamespaceDepth;

                return 0;
            }
        }

        public string IdentifierName
        {
            get { return (syntax is ImportAliasSyntax alias) ? alias.Identifier.Text : null; }
        }

        public INamespaceReferenceSymbol ParentNamespace
        {
            get
            {
                // Get parent from symbol
                if(namespaceSymbol != null)
                    return namespaceSymbol.ParentNamespace;

                return null;
            }
        }

        public IReadOnlyList<INamespaceReferenceSymbol> NamespacesInScope
        {
            get
            {
                // Get namespaces from symbol
                if (namespaceSymbol != null)
                    return namespaceSymbol.NamespacesInScope;

                return null;
            }
        }

        public IReadOnlyList<ITypeReferenceSymbol> TypesInScope
        {
            get
            {
                // Get types from symbol
                if(namespaceSymbol != null)
                    return namespaceSymbol.TypesInScope;

                return null;
            }
        }

        public ITypeReferenceSymbol AliasType
        {
            get { return aliasType; }
        }

        public IReferenceSymbol ParentSymbol
        {
            get { return null; }
        }

        public ITypeReferenceSymbol TypeSymbol
        {
            get { return aliasType; }
        }

        public bool IsAlias
        {
            get { return syntax is ImportAliasSyntax; }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return null; }
        }

        public _TokenHandle Token
        {
            get { return default; }
        }

        // Constructor
        public ImportModel(ImportSyntax syntax)
        {
            this.syntax = syntax;
        }

        // Methods
        public void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve namespace names
            namespaceSymbol = provider.ResolveNamespaceSymbol(syntax.Name, Span);

            // Resolve alias type
            if(IsAlias == true)
            {
                // Resolve alias type
                aliasType = provider.ResolveTypeSymbol(namespaceSymbol, (syntax as ImportAliasSyntax).Type);
            }
        }
    }
}
