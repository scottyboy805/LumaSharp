
using LumaSharp_Compiler.AST;
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

        public abstract IEnumerable<SymbolModel> Descendants { get; }

        // Constructor
        protected SymbolModel(SemanticModel model, SymbolModel parent)
        {
            this.model = model;
            this.parent = parent;
        }

        // Methods
        public abstract void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report);

        public IEnumerable<T> DescendantsOfType<T>(bool withChildren = false) where T : SymbolModel
        {
            foreach (SymbolModel node in Descendants)
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
            foreach (SymbolModel node in Descendants)
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
    }
}
