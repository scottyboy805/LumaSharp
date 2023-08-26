
namespace LumaSharp_Compiler.Semantics.Model
{
    public abstract class SymbolModel// : ModelNode
    {
        // Private
        private SemanticModel model = null;

        // Properties
        public SemanticModel Model
        {
            get { return model; }
        }

        //public abstract IReferenceSymbol ResolvedSymbol { get; }

        // Constructor
        protected SymbolModel(SemanticModel model)
        {
            this.model = model;
        }

        // Methods
        public abstract bool ResolveSymbols(ISymbolProvider provider);
    }
}
