using LumaSharp_Compiler.Semantics.Reference;
using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class SemanticModel : ILibraryReferenceSymbol
    {
        // Private
        private string libraryName = "";
        private TypeModel[] typeModels = null;

        // Properties
        public int SymbolToken
        {
            get { return -1; }  // -1 = current library
        }

        public string LibraryName
        {
            get { return libraryName; }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return this; }
        }

        // Constructor
        internal SemanticModel(string libraryName, IEnumerable<ImportSyntax> importSyntax, IEnumerable<TypeSyntax> rootTypes, IEnumerable<ContractSyntax> rootContracts, IEnumerable<EnumSyntax> rootEnums, IEnumerable<NamespaceSyntax> rootNamespaces)
            : base()
        {
            this.libraryName = libraryName;

            // Create root types
            typeModels = rootTypes != null
                ? rootTypes.Select(t => new TypeModel(this, null, t)).ToArray()
                : null;

            // Create root contracts

        }

        // Methods
        private void Build(string[] references)
        {
            // Create provider
            GlobalSymbolProvider symbolProvider = new GlobalSymbolProvider();

            // Build all members
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
