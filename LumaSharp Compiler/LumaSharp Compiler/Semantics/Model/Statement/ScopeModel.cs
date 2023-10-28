//using LumaSharp_Compiler.AST;
//using LumaSharp_Compiler.Reporting;

//namespace LumaSharp_Compiler.Semantics.Model.Statement
//{
//    public sealed class ScopeModel : StatementModel, IScopedReferenceSymbol
//    {
//        // Private
//        private BlockSyntax<StatementSyntax> block = null;
//        private StatementModel[] statements = null;
//        private ILocalIdentifierReferenceSymbol[] localsInScope = null;

//        // Properties
//        public string ScopeName
//        {
//            get { return "Block Scope"; }
//        }

//        public ILocalIdentifierReferenceSymbol[] LocalsInScope
//        {
//            get { return localsInScope; }
//        }

//        // Constructor
//        public ScopeModel(SemanticModel model, SymbolModel parent, BlockSyntax<StatementSyntax> block)
//            : base(model, parent, block)
//        {
//            this.block = block;

//            // Create locals
//        }

//        // Methods
//        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
//        {

//        }
//    }
//}
