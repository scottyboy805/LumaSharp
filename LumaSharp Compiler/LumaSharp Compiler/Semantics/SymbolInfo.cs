
using System.Diagnostics.CodeAnalysis;

namespace LumaSharp.Compiler.Semantics
{
    public readonly struct SymbolInfo
    {
        // Private
        private readonly ISymbol symbol;
        private readonly ISymbol[] candidateSymbols;

        // Properties
        public readonly ISymbol Symbol => symbol;
        public readonly IReadOnlyList<ISymbol> CandidateSymbols => candidateSymbols;

        public readonly bool HasSymbol => symbol != null;
        public readonly bool HasCandidates => candidateSymbols != null;

        // Constructor
        internal SymbolInfo(ISymbol symbol, ISymbol[] candidateSymbols = null)
        {
            this.symbol = symbol;
            this.candidateSymbols = candidateSymbols;
        }

        // Methods
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            // Check for match
            if(obj is SymbolInfo info)
                return info.symbol == symbol && info.candidateSymbols == candidateSymbols;

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(symbol, candidateSymbols);
        }
    }
}
