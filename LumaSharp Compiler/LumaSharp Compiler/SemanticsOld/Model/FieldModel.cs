using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.SemanticsOld.Reference;

namespace LumaSharp.Compiler.SemanticsOld.Model
{
    internal sealed class FieldModel : MemberModel, IFieldReferenceSymbol, IIdentifierReferenceSymbol
    {
        // Private
        private readonly TypeModel declaringType;
        private readonly TypeReferenceModel fieldTypeModel;
        private readonly ExpressionModel assignModel;     

        // Properties
        public string FieldName
        {
            get { return MemberName; }
        }

        public string IdentifierName
        {
            get { return MemberName; }
        }

        public ITypeReferenceSymbol DeclaringTypeSymbol
        {
            get { return declaringType; }
        }

        public IReferenceSymbol ParentSymbol
        {
            get { return declaringType; }
        }

        public ITypeReferenceSymbol TypeSymbol
        {
            get { return fieldTypeModel.EvaluatedTypeSymbol; }
        }

        public ITypeReferenceSymbol FieldTypeSymbol
        {
            get { return fieldTypeModel.EvaluatedTypeSymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal FieldModel(FieldSyntax fieldSyntax, TypeModel declaringType)
            : base(fieldSyntax != null ? fieldSyntax.Identifier : default, 
                  fieldSyntax != null ? fieldSyntax.Modifiers : null, 
                  fieldSyntax != null ? fieldSyntax.GetSpan() : null)
        {
            // Check for null
            if(fieldSyntax == null)
                throw new ArgumentNullException(nameof(fieldSyntax));

            this.declaringType = declaringType;
            this.fieldTypeModel = new TypeReferenceModel(fieldSyntax.FieldType);
            this.assignModel = fieldSyntax.HasFieldAssignment == true
                ? ExpressionModel.Any(fieldSyntax.FieldAssignment, this)
                : null;

            // Set parent
            fieldTypeModel.parent = this;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitField(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, IDiagnosticReporter report)
        {
            // Resolve base
            base.ResolveSymbols(provider, report);

            // Resolve base symbols
            base.ResolveSymbols(provider, report);

            // Resolve field symbol
            fieldTypeModel.ResolveSymbols(provider, report);

            // Check for assignment
            if(assignModel != null)
            {
                // Resolve assignment symbols
                assignModel.ResolveSymbols(provider, report);

                // Check for assigned and valid conversion
                if(fieldTypeModel.EvaluatedTypeSymbol != null && assignModel.EvaluatedTypeSymbol != null)
                {
                    // Check for return type conversion
                    if (TypeChecker.IsTypeAssignable(assignModel.EvaluatedTypeSymbol, fieldTypeModel.EvaluatedTypeSymbol) == false)
                    {
                        report.Report(Code.InvalidConversion, DiagnosticSeverity.Error, assignModel.Span, fieldTypeModel.EvaluatedTypeSymbol, assignModel.EvaluatedTypeSymbol);
                    }
                }
            }
        }
    }
}
