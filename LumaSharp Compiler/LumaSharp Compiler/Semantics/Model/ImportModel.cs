using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class ImportModel : IAliasIdentifierReferenceSymbol
    {
        // Private
        private ImportSyntax syntax = null;
        private ITypeReferenceSymbol aliasType = null;

        // Properties
        public string[] Namespace
        {
            get
            {
                // Check for type
                if (syntax.Name != null)
                    return syntax.Name.Identifiers.Select(i => i.Text).ToArray();

                return null;
            }
        }

        public string IdentifierName
        {
            get { return syntax.AliasIdentifier.Text; }
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
            get { return syntax.HasAlias; }
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

            // Resolve alias type
            if(IsAlias == true)
            {
                // Resolve alias type
                aliasType = provider.ResolveTypeSymbol(null, syntax.AliasTypeReference
                    .WithNamespaceQualifier(Namespace));
            }
        }
    }
}
