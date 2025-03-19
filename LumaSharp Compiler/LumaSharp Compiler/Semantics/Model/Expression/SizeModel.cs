using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class SizeModel : ExpressionModel
    {
        // Private
        private SizeExpressionSyntax syntax = null;
        private TypeReferenceModel typeModel = null;
        private ITypeReferenceSymbol returnSymbol = null;

        // Properties
        public override bool IsStaticallyEvaluated
        {
            get { return false; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return returnSymbol; }
        }

        public ITypeReferenceSymbol SizeTypeSymbol
        {
            get { return typeModel.EvaluatedTypeSymbol; }
        }

        public TypeReferenceModel SizeTypeExpression
        {
            get { return typeModel; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield return typeModel; }
        }

        // Constructor
        public SizeModel(SemanticModel model, SymbolModel parent, SizeExpressionSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.typeModel = new TypeReferenceModel(model, this, syntax.TypeReference);
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitSize(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve type
            typeModel.ResolveSymbols(provider, report);

            // Resolve return type
            returnSymbol = provider.ResolveTypeSymbol(PrimitiveType.I32, syntax.StartToken.Source);
        }
    }
}
