using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model.Expression
{
    public sealed class ThisModel : ExpressionModel
    {
        // Private
        private ThisExpressionSyntax syntax = null;
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
        public ThisModel(SemanticModel model, SymbolModel parent, ThisExpressionSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
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
            while (current.Parent != null && (current is MethodModel) == false)
                current = current.Parent;

            // Get method scope
            IMethodReferenceSymbol thisMethodScope = current as IMethodReferenceSymbol;

            if (thisMethodScope != null)
            {
                // Get type symbol
                thisTypeReference = thisMethodScope.DeclaringTypeSymbol;
            }

            // Must be invalid usage context
            if(thisMethodScope == null || thisMethodScope.IsGlobal == true)
            {
                report.ReportMessage(Code.KeywordNotValid, MessageSeverity.Error, syntax.StartToken.Source, syntax.Keyword.Text);
            }
        }
    }
}
