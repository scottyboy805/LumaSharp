using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model.Expression
{
    public sealed class TypeReferenceModel : ExpressionModel
    {
        // Private
        private TypeReferenceSyntax syntax = null;
        private ITypeReferenceSymbol typeSymbol = null;

        // Properties
        public override bool IsStaticallyEvaluated
        {
            get { return true; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return typeSymbol; }
        }

        // Constructor
        internal TypeReferenceModel(SemanticModel model, SymbolModel parent, TypeReferenceSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Try to resolve symbol
            this.typeSymbol = provider.ResolveTypeSymbol(ParentSymbol, syntax);
        }
    }
}
