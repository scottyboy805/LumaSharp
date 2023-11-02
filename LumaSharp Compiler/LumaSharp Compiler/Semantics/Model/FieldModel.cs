
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model.Expression;
using LumaSharp_Compiler.Semantics.Model.Statement;
using LumaSharp_Compiler.Semantics.Reference;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class FieldModel : MemberModel, IFieldReferenceSymbol, IIdentifierReferenceSymbol
    {
        // Private
        private FieldSyntax syntax = null;
        private TypeModel declaringType = null;
        private ExpressionModel assignModel = null;
        private ITypeReferenceSymbol fieldTypeSymbol = null;

        // Properties
        public string FieldName
        {
            get { return syntax.Identifier.Text; }
        }

        public string IdentifierName
        {
            get { return syntax.Identifier.Text; }
        }

        public bool IsGlobal
        {
            get { return syntax.HasAccessModifiers == true && syntax.AccessModifiers.FirstOrDefault(m => m.Text == "global") != null; }
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
            get { return fieldTypeSymbol; }
        }

        public ITypeReferenceSymbol FieldTypeSymbol
        {
            get { return fieldTypeSymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal FieldModel(SemanticModel model, TypeModel parent, FieldSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.declaringType = parent;
            this.assignModel = syntax.HasFieldAssignment == true
                ? ExpressionModel.Any(model, this, syntax.FieldAssignment)
                : null;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitField(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve base symbols
            base.ResolveSymbols(provider, report);

            // Resolve field symbol
            fieldTypeSymbol = provider.ResolveTypeSymbol(declaringType, syntax.FieldType);

            // Check for assignment
            if(assignModel != null)
            {
                // Resolve assignment symbols
                assignModel.ResolveSymbols(provider, report);

                // Check for assigned and valid conversion
                if(fieldTypeSymbol != null && assignModel.EvaluatedTypeSymbol != null)
                {
                    // Check for return type conversion
                    if (TypeChecker.IsTypeAssignable(assignModel.EvaluatedTypeSymbol, fieldTypeSymbol) == false)
                    {
                        report.ReportMessage(Code.InvalidConversion, MessageSeverity.Error, syntax.StartToken.Source, fieldTypeSymbol, assignModel.EvaluatedTypeSymbol);
                    }
                }
            }
        }

        public override void StaticallyEvaluateMember(ISymbolProvider provider)
        {
            // Check for expression which can be statically evaluated
            if(assignModel != null && assignModel.IsStaticallyEvaluated == true)
            {
                // Evaluate the expression
                assignModel = assignModel.StaticallyEvaluateExpression(provider);
            }
        }
    }
}
