
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model.Expression
{
    public sealed class VariableReferenceModel : ExpressionModel
    {
        // Private
        private VariableReferenceExpressionSyntax syntax = null;
        private IIdentifierReferenceSymbol identifierSymbol = null;

        // Properties
        public string Identifier
        {
            get { return syntax.Identifier.Text; }
        }

        public override bool IsStaticallyEvaluated
        {
            get { return false; }
        }

        public IIdentifierReferenceSymbol IdentifierSymbol
        {
            get { return identifierSymbol; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get
            {
                if (identifierSymbol != null)
                    return identifierSymbol.TypeSymbol;

                return null;
            }
        }

        // Constructor
        internal VariableReferenceModel(SemanticModel model, SymbolModel parent, VariableReferenceExpressionSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Try to resolve symbol
            this.identifierSymbol = provider.ResolveIdentifierSymbol(ParentSymbol, syntax);
        }
    }
}
