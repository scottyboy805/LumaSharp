
namespace LumaSharp.Compiler.Semantics
{
    internal readonly struct SymbolBinding
    {
        // Private
        private readonly ISymbol symbol;
        private readonly ISymbol[] candidateSymbols;
        private readonly ITypeSymbol inferredType;
        private readonly IConstantSymbol constant;

        // Properties
        public ISymbol Symbol => symbol;
        public IReadOnlyList<ISymbol> CandidateSymbols => candidateSymbols;
        public ITypeSymbol InferredType => inferredType;
        public IConstantSymbol Constant => constant;

        // Constructor
        public SymbolBinding(ISymbol symbol, ITypeSymbol inferredType = null, IConstantSymbol constant = null)
        {
            this.symbol = symbol;
            this.candidateSymbols = null;
            this.inferredType = inferredType;
            this.constant = constant;
        }

        public SymbolBinding(ISymbol[] candidateSymbols, ITypeSymbol inferredType = null)
        {
            this.symbol = null;
            this.candidateSymbols = candidateSymbols;
            this.inferredType = inferredType;
            this.constant = null;
        }

        // Methods
        public SymbolInfo GetSymbolInfo()
        {
            return new SymbolInfo(symbol, candidateSymbols);
        }
    }
}
