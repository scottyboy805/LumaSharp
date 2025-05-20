using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime.Reflection;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Reference;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class FieldModel : MemberModel, IFieldReferenceSymbol, IIdentifierReferenceSymbol
    {
        // Private
        private FieldSyntax syntax = null;
        private TypeModel declaringType = null;
        private ExpressionModel assignModel = null;
        private TypeReferenceModel fieldTypeModel = null;

        private FieldFlags fieldFlags = default;
        private _FieldHandle fieldHandle = default;

        // Properties
        public string FieldName
        {
            get { return syntax.Identifier.Text; }
        }

        public string IdentifierName
        {
            get { return syntax.Identifier.Text; }
        }

        public bool IsExport
        {
            get { return syntax.HasAccessModifiers == true && syntax.AccessModifiers.Any(m => m.Kind == SyntaxTokenKind.ExportKeyword); }
        }

        public bool IsInternal
        {
            get { return syntax.HasAccessModifiers == true && syntax.AccessModifiers.Any(m => m.Kind == SyntaxTokenKind.InternalKeyword); }
        }

        public bool IsHidden
        {
            get { return syntax.HasAccessModifiers == true && syntax.AccessModifiers.Any(m => m.Kind == SyntaxTokenKind.HiddenKeyword); }
        }

        public bool IsGlobal
        {
            get { return syntax.HasAccessModifiers == true && syntax.AccessModifiers.Any(m => m.Kind == SyntaxTokenKind.GlobalKeyword); }
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

        public FieldFlags FieldFlags
        {
            get { return fieldFlags; }
        }

        public _FieldHandle FieldHandle
        {
            get { return fieldHandle; }
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
            this.fieldTypeModel = new TypeReferenceModel(model, this, syntax.FieldType);
            this.assignModel = syntax.HasFieldAssignment == true
                ? ExpressionModel.Any(model, this, syntax.FieldAssignment)
                : null;

            // Create flags
            this.fieldFlags = BuildFieldFlags();
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitField(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Get symbol token
            memberToken = provider.GetDeclaredSymbolToken(this);

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
                        report.ReportDiagnostic(Code.InvalidConversion, MessageSeverity.Error, syntax.StartToken.Source, fieldTypeModel.EvaluatedTypeSymbol, assignModel.EvaluatedTypeSymbol);
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

        private FieldFlags BuildFieldFlags()
        {
            FieldFlags flags = 0;

            // Check for export
            if (IsExport == true) flags |= FieldFlags.Export;

            // Check for internal
            if (IsInternal == true) flags |= FieldFlags.Internal;

            // Check for hidden
            if (IsHidden == true) flags |= FieldFlags.Hidden;

            // Check for global
            if (IsGlobal == true) flags |= FieldFlags.Global;

            return flags;
        }
    }
}
