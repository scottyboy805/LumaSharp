using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public abstract class SymbolModel
    {
        // Internal
        internal SymbolModel parent;

        // Protected
        protected readonly SyntaxSpan? span;

        // Properties
        public SemanticModel Model
        {
            get { return parent?.Model; }
        }

        public SymbolModel Parent
        {
            get { return parent; }
        }

        public SyntaxSpan? Span
        {
            get { return span; }
        }

        public abstract IEnumerable<SymbolModel> Descendants { get; }

        // Constructor
        protected SymbolModel(SyntaxSpan? span) 
        {
            this.span = span;
        }

        // Methods
        public abstract void Accept(ISemanticVisitor visitor);

        public abstract void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report);

        public IEnumerable<T> DescendantsOfType<T>(bool withChildren = false) where T : class
        {
            foreach (SymbolModel node in Descendants)
            {
                if (node is T)
                    yield return node as T;

                // Check for children
                if (withChildren == true)
                {
                    foreach (T child in node.DescendantsOfType<T>(withChildren))
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
