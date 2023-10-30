using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model.Expression
{
    public sealed class FieldAccessorReferenceModel : ExpressionModel
    {
        // Private
        private FieldAccessorReferenceExpressionSyntax syntax = null;
        private ExpressionModel accessModel = null;
        private IIdentifierReferenceSymbol fieldAccessorIdentifierSymbol = null;

        // Properties
        public override bool IsStaticallyEvaluated
        {
            get { return accessModel.IsStaticallyEvaluated; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get 
            { 
                // Check for resolved identifier
                if(fieldAccessorIdentifierSymbol != null)
                    return fieldAccessorIdentifierSymbol.TypeSymbol;

                return null;
            }
        }

        public ITypeReferenceSymbol FieldAccessorTypeSymbol
        {
            get
            {
                // Check for resolved identifier
                if (fieldAccessorIdentifierSymbol != null)
                    return fieldAccessorIdentifierSymbol.TypeSymbol;

                return null;
            }
        }

        public IIdentifierReferenceSymbol FieldAccessorIdentifier
        {
            get { return fieldAccessorIdentifierSymbol; }
        }

        public ExpressionModel AccessModelExpression
        {
            get { return accessModel; }
        }

        public bool IsFieldReference
        {
            get { return fieldAccessorIdentifierSymbol is IFieldReferenceSymbol; }
        }

        public bool IsAccessorReference
        {
            get { return fieldAccessorIdentifierSymbol is IAccessorReferenceSymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield return accessModel; }
        }

        // Constructor
        public FieldAccessorReferenceModel(SemanticModel model, SymbolModel parent, FieldAccessorReferenceExpressionSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.accessModel = ExpressionModel.Any(model, this, syntax.AccessExpression);
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve accessor
            accessModel.ResolveSymbols(provider, report);

            // Resolve field if accessor is valid
            if(accessModel.EvaluatedTypeSymbol != null)
            {
                fieldAccessorIdentifierSymbol = provider.ResolveFieldIdentifierSymbol(accessModel.EvaluatedTypeSymbol, syntax);
            }
        }
    }
}
