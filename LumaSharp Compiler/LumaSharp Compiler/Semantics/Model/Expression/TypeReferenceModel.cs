using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Reference;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class TypeReferenceModel : ExpressionModel
    {
        // Private
        private TypeReferenceSyntax syntax = null;
        private ITypeReferenceSymbol typeSymbol = null;
        private TypeReferenceModel[] genericArgumentModels= null;

        // Properties
        public TypeReferenceSyntax Syntax
        {
            get { return syntax; }
        }

        public override bool IsStaticallyEvaluated
        {
            get { return false; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return typeSymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal TypeReferenceModel(SemanticModel model, SymbolModel parent, TypeReferenceSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;

            if (syntax.GenericArgumentCount > 0)
            {
                this.genericArgumentModels = syntax.GenericArguments.Select(
                    t => new TypeReferenceModel(model, this, t)).ToArray();
            }
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitTypeReference(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Try to resolve symbol
            this.typeSymbol = provider.ResolveTypeSymbol(ParentSymbol, syntax);

            // Resolve generic arguments
            if (genericArgumentModels != null)
            {
                for (int i = 0; i < genericArgumentModels.Length; i++)
                {
                    genericArgumentModels[i].ResolveSymbols(provider, report);
                }
            }

            // Check for generic argument usage
            if (typeSymbol != null)
            {
                // Check for generic type
                if (syntax.IsGenericType == true && genericArgumentModels != null)
                {
                    // Check all generic arguments
                    CheckGenericArguments(typeSymbol, syntax.GenericArguments.ToArray(), genericArgumentModels, report);
                }
            }
        }

        private void CheckGenericArguments(ITypeReferenceSymbol typeSymbol, TypeReferenceSyntax[] genericArgumentTypes, TypeReferenceModel[] genericArgumentTypeSymbols, ICompileReportProvider report)
        {
            // Check for generic argument mismatch
            if(typeSymbol.GenericParameterSymbols == null)
            {
                report.ReportMessage(Code.InvalidNoGenericArgument, MessageSeverity.Error, genericArgumentTypes[0].StartToken.Source, typeSymbol);
                return;
            }

            // Check for mismatch generic argument count
            if(typeSymbol.GenericParameterSymbols.Length != genericArgumentTypes.Length)
            {
                report.ReportMessage(Code.InvalidCountGenericArgument, MessageSeverity.Error, genericArgumentTypes[0].StartToken.Source, typeSymbol, genericArgumentTypes.Length);
                return;
            }

            // Check all generic arguments
            for(int i = 0; i < genericArgumentTypeSymbols.Length; i++)
            {
                CheckGenericArgument(typeSymbol.GenericParameterSymbols[i], genericArgumentTypes[i], genericArgumentTypeSymbols[i], report);
            }
        }

        private void CheckGenericArgument(IGenericParameterIdentifierReferenceSymbol genericParameter, TypeReferenceSyntax syntax, TypeReferenceModel genericArgument, ICompileReportProvider report)
        {
            // Check for any constraints
            if(genericParameter.TypeConstraintSymbols != null && genericParameter.TypeConstraintSymbols.Length > 0)
            {
                // Make sure all constraints are implemented
                foreach(ITypeReferenceSymbol genericConstraint in genericParameter.TypeConstraintSymbols)
                {
                    // Check for assignable
                    if(TypeChecker.IsTypeAssignable(genericArgument.EvaluatedTypeSymbol, genericConstraint) == false)
                    {
                        // Constraint is not implemented
                        report.ReportMessage(Code.InvalidConstraintGenericArgument, MessageSeverity.Error, syntax.StartToken.Source, genericArgument, genericConstraint);
                    }
                }
            }
        }
    }
}
