using LumaSharp_Compiler.Semantics.Reference;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class SemanticModel : ILibraryReferenceSymbol
    {
        // Private
        private string libraryName = "";
        private ReferenceLibrary thisLibrary = null;
        private List<TypeModel> typeModels = new List<TypeModel>();
        private CompileReport report = null;

        // Properties
        public string LibraryName
        {
            get { return libraryName; }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return thisLibrary; }
        } 

        public int SymbolToken
        {
            get { return thisLibrary.SymbolToken; }
        }

        public IReadOnlyList<TypeModel> TypeModels
        {
            get { return typeModels; }
        }

        public ICompileReportProvider Report
        {
            get { return report; }
        }

        // Constructor
        internal SemanticModel(string libraryName, IEnumerable<ImportSyntax> importSyntax, IEnumerable<TypeSyntax> rootTypes, IEnumerable<ContractSyntax> rootContracts, IEnumerable<EnumSyntax> rootEnums, IEnumerable<NamespaceSyntax> rootNamespaces)
            : base()
        {
            this.libraryName = libraryName;

            // Create root types
            if (rootTypes != null)
                typeModels.AddRange(rootTypes.Select(t => new TypeModel(this, null, t)));

            // Create root contracts
            if (rootContracts != null)
                typeModels.AddRange(rootContracts.Select(c => new TypeModel(this, null, c)));

            // Create root enums
            if (rootEnums != null)
                typeModels.AddRange(rootEnums.Select(e => new TypeModel(this, null, e)));

            // Create namespace members
            if (rootNamespaces != null)
            {
                foreach (NamespaceSyntax ns in rootNamespaces)
                {
                    //// Build the namespace model
                    //NamespaceModel nsModel = new NamespaceModel(this, null, ns);

                    //// Register all types
                    //foreach (TypeModel type in nsModel.TypesInScope)
                    //{
                    //    typeModels.Add(type);
                    //}

                    // Add all types
                    typeModels.AddRange(ns.DescendantsOfType<TypeSyntax>().Select(t => new TypeModel(this, null, t)));

                    // Add all contracts
                    typeModels.AddRange(ns.DescendantsOfType<ContractSyntax>().Select(c => new TypeModel(this, null, c)));

                    // Add all enums
                    typeModels.AddRange(ns.DescendantsOfType<EnumSyntax>().Select(e => new TypeModel(this, null, e)));
                }
            }
        }

        // Methods
        private void Build(string[] references)
        {
            // Create library
            this.thisLibrary = new ReferenceLibrary(libraryName);

            // Register all types
            foreach(TypeModel type in typeModels)
            {
                if (type.NamespaceSymbol != null)
                {
                    // Declare a named type - will only add the namespace if it does not yet exist
                    thisLibrary.DeclareNamedType(type);
                }
                else
                {
                    // Declare this type
                    thisLibrary.DeclareType(type);
                }
            }


            // Load all external references
            if(references != null)
            {

            }

            // Create provider
            ReferenceSymbolProvider symbolProvider = new ReferenceSymbolProvider(thisLibrary, report);

            // Build all external members


            // Resolve symbols
            foreach(TypeModel type in typeModels)
            {
                // Resolve the symbols
                type.ResolveSymbols(symbolProvider, report);
            }
        }

        public static SemanticModel BuildModel(string libraryName, SyntaxTree[] syntaxTrees, string[] references)
        {
            // Concat all trees
            List<ImportSyntax> imports = new List<ImportSyntax>();
            List<TypeSyntax> rootTypes = new List<TypeSyntax>();
            List<ContractSyntax> rootContracts = new List<ContractSyntax>();
            List<EnumSyntax> rootEnums = new List<EnumSyntax>();
            List<NamespaceSyntax> rootNamespaces = new List<NamespaceSyntax>();

            // Get combined report
            CompileReport report = syntaxTrees.Length > 0
                ? (CompileReport)syntaxTrees[0].Report
                : new CompileReport();

            int index = 0;

            // Process all trees
            foreach (SyntaxTree tree in syntaxTrees)
            {
                // Add imports
                imports.AddRange(tree.DescendantsOfType<ImportSyntax>());

                // Add root types
                rootTypes.AddRange(tree.DescendantsOfType<TypeSyntax>());

                // Add root contracts
                rootContracts.AddRange(tree.DescendantsOfType<ContractSyntax>());

                // Add root enums
                rootEnums.AddRange(tree.DescendantsOfType<EnumSyntax>());

                // Add namespace
                rootNamespaces.AddRange(tree.DescendantsOfType<NamespaceSyntax>());

                // Combine the report
                if (index > 1) report.Combine(tree.Report);
                index++;
            }

            // Create the model
            SemanticModel model = new SemanticModel(libraryName, imports, rootTypes, rootContracts, rootEnums, rootNamespaces);

            // Store report
            model.report = report;

            // Build the model
            model.Build(references);

            return model;
        }
    }
}
