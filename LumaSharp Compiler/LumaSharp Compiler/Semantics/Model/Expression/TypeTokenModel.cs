using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class TypeTokenModel : ExpressionModel
    {
        // Private
        private TypeExpressionSyntax syntax = null;
        private TypeReferenceModel typeTokenModel = null;

        // Properties
        public override bool IsStaticallyEvaluated
        {
            get { return false; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return typeTokenModel.EvaluatedTypeSymbol; }
        }

        public TypeReferenceModel TypeTokenExpression
        {
            get { return typeTokenModel; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield return typeTokenModel; }
        }

        // Constructor
        public TypeTokenModel(SemanticModel model, SymbolModel parent, TypeExpressionSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.typeTokenModel = new TypeReferenceModel(model, this, syntax.TypeReference);
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitTypeToken(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve type
            typeTokenModel.ResolveSymbols(provider, report);
        }
    }
}
