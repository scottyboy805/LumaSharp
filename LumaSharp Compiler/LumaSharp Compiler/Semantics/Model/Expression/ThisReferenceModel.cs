using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model.Expression
{
    public sealed class ThisReferenceModel : ExpressionModel
    {
        // Private
        private ITypeReferenceSymbol thisTypeReference = null;

        // Properties
        public override bool IsStaticallyEvaluated
        {
            get { return true; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return thisTypeReference; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        // Constructor
        public ThisReferenceModel(SemanticModel model, SymbolModel parent, ThisExpressionSyntax syntax)
            : base(model, parent, syntax)
        {
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitThis(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Get parent type
            SymbolModel current = this;

            // Move up the hierarchy
            while (current.Parent != null && (current is TypeModel) == false)
                current = current.Parent;

            // Get type symbol
            thisTypeReference = current as ITypeReferenceSymbol;
        }
    }
}
