using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class NewModel : ExpressionModel
    {
        // Private
        private NewExpressionSyntax syntax = null;
        private TypeReferenceModel newTypeModel = null;

        // Properties
        public override bool IsStaticallyEvaluated
        {
            get { return false; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return newTypeModel.EvaluatedTypeSymbol; }
        }

        public TypeReferenceModel NewTypeExpression
        {
            get { return newTypeModel; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield return newTypeModel; }
        }

        // Constructor
        public NewModel(SemanticModel model, SymbolModel parent, NewExpressionSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.newTypeModel = new TypeReferenceModel(model, this, syntax.NewType);
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitNew(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve type
            newTypeModel.ResolveSymbols(provider, report);
        }
    }
}
