using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics
{
    public sealed class SemanticModel
    {
        // Private
        private readonly SymbolBuilder builder = null;
        private readonly List<SyntaxTree> syntaxTrees = new();        
        private readonly Dictionary<SyntaxNode, SymbolBinding> cachedSymbols = new();

        // Properties
        public IReadOnlyList<SyntaxTree> SyntaxTrees => syntaxTrees;
        internal SymbolBinder Binder => builder.Binder;

        // Constructor
        internal SemanticModel(IEnumerable<SyntaxTree> syntaxTrees)
        {
            // Check for any
            if(syntaxTrees == null)
                throw new ArgumentNullException(nameof(syntaxTrees));

            this.builder = new SymbolBuilder(this);
            this.syntaxTrees.AddRange(syntaxTrees);

            // Require atleast 1
            if (this.syntaxTrees.Count == 0)
                throw new ArgumentException("Must provide 1 or more syntax trees");            
        }

        // Methods
        public SymbolInfo GetSymbol(SyntaxNode node)
        {
            // Get the bound symbol with caching
            SymbolBinding binding = GetBoundSymbol(node);

            // Try to get symbol info
            return binding.GetSymbolInfo();
        }

        public IOperation GetOperation(SyntaxNode node)
        {
            // Get the bound symbol with caching
            SymbolBinding binding = GetBoundSymbol(node);

            // Try to get operation
            return binding.Operation;
        }

        public IConstantSymbol GetConstantSymbol(SyntaxNode node)
        {
            // Get the bound symbol with caching
            SymbolBinding binding = GetBoundSymbol(node);

            // Try to get constant
            return binding.Constant;
        }

        public ITypeSymbol GetTypeSymbol(SyntaxNode node)
        {
            // Get the bound symbol with caching
            SymbolBinding binding = GetBoundSymbol(node);

            // Inferred type is ok as a fallback
            return binding.InferredType;
        }

        private SymbolBinding GetBoundSymbol(SyntaxNode node)
        {
            // Check for null
            if (node == null || IsNodeIncluded(node) == false)
                return default;

            // Check for symbol
            if (cachedSymbols.TryGetValue(node, out SymbolBinding binding) == false)
            {
                // Try to build thr symbol
                binding = node.Accept(builder);
                cachedSymbols[node] = binding;
            }
            return binding;
        }

        private bool IsNodeIncluded(SyntaxNode node)
        {
#warning fix this - parent is null in many cases when it should not be
            return true;

            // Check if this node is part of the syntax trees provided
            if (node != null)
                return syntaxTrees.Any(s => node.SyntaxTree == s);

            // The node is part of a separate syntax tree
            return false;
        }
    }
}
