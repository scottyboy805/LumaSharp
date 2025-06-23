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
        private readonly TypeModel declaringType;
        private readonly TypeReferenceModel fieldTypeModel;
        private readonly ExpressionModel assignModel;        

        private FieldFlags fieldFlags = default;
        private _TokenHandle fieldToken = default;

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

        public FieldFlags FieldFlags
        {
            get { return fieldFlags; }
        }

        public override _TokenHandle Token
        {
            get { return fieldToken; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal FieldModel(FieldSyntax fieldSyntax, TypeModel declaringType)
            : base(fieldSyntax != null ? fieldSyntax.Identifier : default, 
                  fieldSyntax != null ? fieldSyntax.AccessModifiers : null, 
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
            // Resolve base
            base.ResolveSymbols(provider, report);

            // Get symbol token
            fieldToken = provider.GetSymbolToken(this);

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
                        report.ReportDiagnostic(Code.InvalidConversion, MessageSeverity.Error, assignModel.Span, fieldTypeModel.EvaluatedTypeSymbol, assignModel.EvaluatedTypeSymbol);
                    }
                }
            }
        }

        //public override void StaticallyEvaluateMember(ISymbolProvider provider)
        //{
        //    // Check for expression which can be statically evaluated
        //    if(assignModel != null && assignModel.IsStaticallyEvaluated == true)
        //    {
        //        // Evaluate the expression
        //        assignModel = assignModel.StaticallyEvaluateExpression(provider);
        //    }
        //}

        private FieldFlags BuildFieldFlags()
        {
            FieldFlags flags = 0;

            // Check for export
            if (HasAccessModifier(AccessModifier.Export) == true) flags |= FieldFlags.Export;

            // Check for internal
            if (HasAccessModifier(AccessModifier.Internal) == true) flags |= FieldFlags.Internal;

            // Check for hidden
            if (HasAccessModifier(AccessModifier.Hidden) == true) flags |= FieldFlags.Hidden;

            // Check for global
            if (HasAccessModifier(AccessModifier.Global) == true) flags |= FieldFlags.Global;

            return flags;
        }
    }
}
