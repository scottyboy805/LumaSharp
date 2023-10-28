
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model
{
    public abstract class SymbolModel// : ModelNode
    {
        // Private
        private SemanticModel model = null;
        private SymbolModel parent = null;

        // Properties
        public SemanticModel Model
        {
            get { return model; }
        }

        public SymbolModel Parent
        {
            get { return parent; }
        }

        //public abstract IReferenceSymbol ResolvedSymbol { get; }

        // Constructor
        protected SymbolModel(SemanticModel model, SymbolModel parent)
        {
            this.model = model;
            this.parent = parent;
        }

        // Methods
        public abstract void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report);
    }
}
