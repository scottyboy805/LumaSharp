using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class FieldAccessorReferenceModel : ExpressionModel
    {
        // Private
        private MemberAccessExpressionSyntax syntax = null;
        private ExpressionModel accessModel = null;
        private IIdentifierReferenceSymbol fieldAccessorIdentifierSymbol = null;

        // Properties
        public override bool IsStaticallyEvaluated
        {
            get { return false; }
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
        public FieldAccessorReferenceModel(SemanticModel model, SymbolModel parent, MemberAccessExpressionSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.accessModel = ExpressionModel.Any(model, this, syntax.AccessExpression);
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitFieldAccessorReference(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve accessor
            accessModel.ResolveSymbols(provider, report);

            // Resolve field if accessor is valid
            if(accessModel.EvaluatedTypeSymbol != null)
            {
                // Try to resolve the field first of all
                fieldAccessorIdentifierSymbol = provider.ResolveFieldIdentifierSymbol(accessModel.EvaluatedTypeSymbol, syntax);


                // Check for valid access of field
                if (fieldAccessorIdentifierSymbol is IFieldReferenceSymbol fieldIdentifier)
                {
                    // Check for instance field accessed via type reference
                    if(fieldIdentifier.IsGlobal == false && accessModel is TypeReferenceModel)
                    {
                        report.ReportDiagnostic(Code.FieldRequiresInstance, MessageSeverity.Error, syntax.StartToken.Source, fieldIdentifier.IdentifierName);
                    }
                    // Check for global field accessed via anything other than type reference
                    else if(fieldIdentifier.IsGlobal == true && (accessModel is TypeReferenceModel) == false)
                    {
                        report.ReportDiagnostic(Code.FieldRequiresType, MessageSeverity.Error, syntax.StartToken.Source, fieldIdentifier.IdentifierName);
                    }
                }

                
            }
        }
    }
}
