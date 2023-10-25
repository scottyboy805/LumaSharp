using LumaSharp_Compiler.Semantics.Reference;
using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class SemanticModel : ILibraryReferenceSymbol
    {
        // Private
        private string libraryName = "";
        private ReferenceLibrary thisLibrary = null;
        private List<TypeModel> typeModels = new List<TypeModel>();

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
                // Declare this type
                thisLibrary.DeclareType(type.Namespace, type);
            }


            // Load all external references
            if(references != null)
            {

            }

            // Create provider
            ReferenceSymbolProvider symbolProvider = new ReferenceSymbolProvider(thisLibrary);

            // Build all external members


            // Resolve symbols
            foreach(TypeModel type in typeModels)
            {
                // Resolve the symbols
                type.ResolveSymbols(symbolProvider);
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

            // Process all trees
            foreach(SyntaxTree tree in syntaxTrees)
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
            }

            // Create the model
            SemanticModel model = new SemanticModel(libraryName, imports, rootTypes, rootContracts, rootEnums, rootNamespaces);

            // Build the model
            model.Build(references);

            return model;
        }
    }
}
