using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class FieldAccessorReferenceModel : ExpressionModel
    {
        // Private
        private readonly ExpressionModel accessModel;
        private readonly StringModel identifier;
        private IIdentifierReferenceSymbol fieldAccessorIdentifierSymbol = null;

        // Properties
        public StringModel Identifier
        {
            get { return identifier; }
        }

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
        public FieldAccessorReferenceModel(MemberAccessExpressionSyntax memberAccessSyntax)
            : base(memberAccessSyntax != null ? memberAccessSyntax.GetSpan() : null)
        {
            // Check for null
            if(memberAccessSyntax == null)
                throw new ArgumentNullException(nameof(memberAccessSyntax));

            this.accessModel = ExpressionModel.Any(memberAccessSyntax.AccessExpression, this);
            this.identifier = new StringModel(memberAccessSyntax.Identifier);

            // Set parent
            accessModel.parent = this;
        }

        public FieldAccessorReferenceModel(ExpressionModel accessModel, string fieldName, SyntaxSpan? span)
            : base(span)
        {
            // Check for null
            if(accessModel == null)
                throw new ArgumentNullException(nameof(accessModel));

            // Check for empty
            if (string.IsNullOrEmpty(fieldName) == true)
                throw new ArgumentException(nameof(fieldName) + " cannot be null or empty");

            this.accessModel = accessModel;
            this.identifier = new StringModel(fieldName);

            // Set parent
            accessModel.parent = this;
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
                fieldAccessorIdentifierSymbol = provider.ResolveFieldAccessorIdentifierSymbol(accessModel.EvaluatedTypeSymbol, identifier.Text, identifier.Span);


                // Check for valid access of field
                if (fieldAccessorIdentifierSymbol is IFieldReferenceSymbol fieldIdentifier)
                {
                    // Check for instance field accessed via type reference
                    if(fieldIdentifier.IsGlobal == false && accessModel is TypeReferenceModel)
                    {
                        report.ReportDiagnostic(Code.FieldRequiresInstance, MessageSeverity.Error, identifier.Span, fieldIdentifier.IdentifierName);
                    }
                    // Check for global field accessed via anything other than type reference
                    else if(fieldIdentifier.IsGlobal == true && (accessModel is TypeReferenceModel) == false)
                    {
                        report.ReportDiagnostic(Code.FieldRequiresType, MessageSeverity.Error, identifier.Span, fieldIdentifier.IdentifierName);
                    }
                }                
            }
        }
    }
}
