using LumaSharp.Compiler.Semantics.Reference;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class SemanticModel : ILibraryReferenceSymbol
    {
        // Private
        private string libraryName = "";
        private ReferenceSymbolProvider symbolProvider = null;
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

        public _TokenHandle Token
        {
            get { return thisLibrary.Token; }
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
        internal SemanticModel(string libraryName)//, IEnumerable<ImportSyntax> importSyntax, IEnumerable<TypeSyntax> rootTypes, IEnumerable<ContractSyntax> rootContracts, IEnumerable<EnumSyntax> rootEnums, IEnumerable<NamespaceSyntax> rootNamespaces)
            : base()
        {
            this.libraryName = libraryName;

            //// Create root types
            //if (rootTypes != null)
            //    typeModels.AddRange(rootTypes.Select(t => new TypeModel(this, null, t)));

            //// Create root contracts
            //if (rootContracts != null)
            //    typeModels.AddRange(rootContracts.Select(c => new TypeModel(this, null, c)));

            //// Create root enums
            //if (rootEnums != null)
            //    typeModels.AddRange(rootEnums.Select(e => new TypeModel(this, null, e)));

            //// Create namespace members
            //if (rootNamespaces != null)
            //{
            //    foreach (NamespaceSyntax ns in rootNamespaces)
            //    {
            //        //// Build the namespace model
            //        //NamespaceModel nsModel = new NamespaceModel(this, null, ns);

            //        //// Register all types
            //        //foreach (TypeModel type in nsModel.TypesInScope)
            //        //{
            //        //    typeModels.Add(type);
            //        //}

            //        // Add all types
            //        typeModels.AddRange(ns.DescendantsOfType<TypeSyntax>().Select(t => new TypeModel(this, null, t)));

            //        // Add all contracts
            //        typeModels.AddRange(ns.DescendantsOfType<ContractSyntax>().Select(c => new TypeModel(this, null, c)));

            //        // Add all enums
            //        typeModels.AddRange(ns.DescendantsOfType<EnumSyntax>().Select(e => new TypeModel(this, null, e)));
            //    }
            //}
        }

        // Methods
        private void Build(string[] references, SyntaxTree[] syntaxTrees)
        {
            // Create library
            this.thisLibrary = new ReferenceLibrary(libraryName);

            // Get combined report
            CompileReport report = syntaxTrees.Length > 0
                ? (CompileReport)syntaxTrees[0].Report
                : new CompileReport();

            // Store report
            this.report = report;
            int index = 0;

            List<ImportModel> importModels = new List<ImportModel>();
            //List<NamespaceSyntax> namespaceDeclarations = new List<NamespaceSyntax>();

            // Build semantic model
            foreach (SyntaxTree tree in syntaxTrees)
            {
                List<INamespaceReferenceSymbol> scopedImportModels = new List<INamespaceReferenceSymbol>();

                // Add imports
                foreach(ImportSyntax importSyntax in tree.Root.DescendantsOfType<ImportSyntax>())
                {
                    // Create model
                    ImportModel importModel = new ImportModel(importSyntax);

                    // Register model
                    importModels.Add(importModel);
                    scopedImportModels.Add(importModel);
                }

                // Add root namespaces
                //namespaceDeclarations.AddRange(tree.Root.DescendantsOfType<NamespaceSyntax>());

                // Add root types
                typeModels.AddRange(tree.Root.DescendantsOfType<TypeSyntax>().Select(t => new TypeModel(t, null)));
                //rootTypes.AddRange(tree.DescendantsOfType<TypeSyntax>());

                // Add root contracts
                typeModels.AddRange(tree.Root.DescendantsOfType<ContractSyntax>().Select(c => new TypeModel(c, null)));
                //rootContracts.AddRange(tree.DescendantsOfType<ContractSyntax>());

                // Add root enums
                typeModels.AddRange(tree.Root.DescendantsOfType<EnumSyntax>().Select(e => new TypeModel(e, null)));
                //rootEnums.AddRange(tree.DescendantsOfType<EnumSyntax>());

                // Add namespace
                //foreach(NamespaceSyntax rootNamespace in tree.Root.DescendantsOfType<NamespaceSyntax>())
                //{
                //    // Add all types
                //    typeModels.AddRange(rootNamespace.DescendantsOfType<TypeSyntax>().Select(t => new TypeModel(this, null, t, scopedImportModels)));

                //    // Add all contracts
                //    typeModels.AddRange(rootNamespace.DescendantsOfType<ContractSyntax>().Select(c => new TypeModel(this, null, c, scopedImportModels)));

                //    // Add all enums
                //    typeModels.AddRange(rootNamespace.DescendantsOfType<EnumSyntax>().Select(e => new TypeModel(this, null, e, scopedImportModels)));
                //}

                // Combine the report
                if (index > 1) report.Combine(tree.Report);
                index++;
            }


            // Define all namespaces
            //foreach(NamespaceSyntax namespaceSyntax in namespaceDeclarations)
            //{
            //    thisLibrary.DeclareNamespace(namespaceSyntax.Name);
            //}

            // Define this library with all types
            foreach (TypeModel type in typeModels)
            {
                if (type.NamespaceName != null)
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


            // Load runtime reference
            ReferenceLibrary runtimeLibrary = new ReferenceLibrary("runtime");

            // Load all external references
            if(references != null)
            {

            }

            // Create provider
            symbolProvider = new ReferenceSymbolProvider(runtimeLibrary, thisLibrary, report);

            // Build all external members


            // Resolve import symbols first
            foreach(ImportModel import in importModels)
            {
                import.ResolveSymbols(symbolProvider, report);
            }

            // Resolve symbols
            foreach(TypeModel type in typeModels)
            {
                // Resolve the symbols
                type.ResolveSymbols(symbolProvider, report);
            }
        }

        //public void StaticallyEvaluate()
        //{
        //    foreach(TypeModel type in typeModels)
        //    {
        //        type.StaticallyEvaluateMember(symbolProvider);
        //    }
        //}

        public IEnumerable<T> DescendantsOfType<T>(bool withChildren = false) where T : SymbolModel
        {
            foreach (SymbolModel node in typeModels)
            {
                if (node is T)
                    yield return node as T;

                // Check for children
                if (withChildren == true)
                {
                    foreach (SymbolModel child in node.DescendantsOfType<T>(withChildren))
                        if (child is T)
                            yield return child as T;
                }
            }
        }

        public IEnumerable<SymbolModel> DescendantsOfType(Type type, bool withChildren = false)
        {
            foreach (SymbolModel node in typeModels)
            {
                if (type.IsAssignableFrom(node.GetType()) == true)
                    yield return node;

                // Check for children
                if (withChildren == true)
                {
                    foreach (SymbolModel child in node.DescendantsOfType(type, withChildren))
                        if (type.IsAssignableFrom(child.GetType()) == true)
                            yield return child;
                }
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

            //// Get combined report
            //CompileReport report = syntaxTrees.Length > 0
            //    ? (CompileReport)syntaxTrees[0].Report
            //    : new CompileReport();

            //int index = 0;

            //// Process all trees
            //foreach (SyntaxTree tree in syntaxTrees)
            //{
            //    // Add imports
            //    imports.AddRange(tree.DescendantsOfType<ImportSyntax>());

            //    // Add root types
            //    rootTypes.AddRange(tree.DescendantsOfType<TypeSyntax>());

            //    // Add root contracts
            //    rootContracts.AddRange(tree.DescendantsOfType<ContractSyntax>());

            //    // Add root enums
            //    rootEnums.AddRange(tree.DescendantsOfType<EnumSyntax>());

            //    // Add namespace
            //    rootNamespaces.AddRange(tree.DescendantsOfType<NamespaceSyntax>());

            //    // Combine the report
            //    if (index > 1) report.Combine(tree.Report);
            //    index++;
            //}

            // Create the model
            SemanticModel model = new SemanticModel(libraryName);//, imports, rootTypes, rootContracts, rootEnums, rootNamespaces);

            // Build the model
            model.Build(references, syntaxTrees);

            return model;
        }
    }
}
