using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Reference;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class TypeReferenceModel : ExpressionModel
    {
        // Type
        public sealed class ParentTypeReference
        {
            // Private
            private readonly StringModel name;
            private readonly TypeReferenceModel[] genericArgumentModels;

            // Properties
            public StringModel Name
            {
                get { return name; }
            }

            public TypeReferenceModel[] GenericArguments
            {
                get { return genericArgumentModels; }
            }

            public bool IsGenericType
            {
                get { return genericArgumentModels != null; }
            }

            // Constructor
            internal ParentTypeReference(string name, TypeReferenceModel[] genericArgumentModels)
            {
                // Check for null or empty
                if (string.IsNullOrEmpty(name) == true)
                    throw new ArgumentException(nameof(name) + " cannot be null or empty");

                this.name = new StringModel(name);
                this.genericArgumentModels = genericArgumentModels;
            }
        }

        // Private
        private readonly StringModel namespaceName;
        private readonly StringModel typeName;
        private readonly ParentTypeReference[] parentTypes;
        private readonly TypeReferenceModel[] genericArgumentModels;
        private readonly int arrayRank = -1;
        private ITypeReferenceSymbol typeSymbol = null;        

        // Properties
        public StringModel Namespace
        {
            get { return namespaceName; }
        }

        public StringModel TypeName
        {
            get { return typeName; }
        }

        public override bool IsStaticallyEvaluated
        {
            get { return false; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return typeSymbol; }
        }

        public ITypeReferenceSymbol[] EvaluatedGenericTypeSymbols
        {
            get { return genericArgumentModels.Select(g => g.EvaluatedTypeSymbol).ToArray(); }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        public bool IsResolved
        {
            get { return typeSymbol != null; }
        }

        public bool HasNamespace
        {
            get { return namespaceName != null; }
        }

        public bool IsNestedType
        {
            get { return parentTypes != null; }
        }

        public bool IsGenericType
        {
            get { return genericArgumentModels != null; }
        }

        private string[] NamespaceNames
        {
            get 
            {
                return namespaceName != null
                    ? namespaceName.Text.Split(SyntaxToken.GetText(SyntaxTokenKind.ColonSymbol))
                    : null;
            }
        }

        // Constructor
        internal TypeReferenceModel(TypeReferenceSyntax typeSyntax)
            : base(typeSyntax != null ? typeSyntax.GetSpan() : null)
        {
            // Check for null
            if (typeSyntax == null)
                throw new ArgumentNullException(nameof(typeSyntax));

            this.typeName = new StringModel(typeSyntax.Identifier);
            this.namespaceName = typeSyntax.HasNamespace == true
                ? new StringModel(typeSyntax.Namespace.NormalizeWhitespace().ToString())
                : null;        
            this.parentTypes = typeSyntax.IsNested == true
                ? typeSyntax.ParentTypes.Select(p => new ParentTypeReference(p.Identifier.Text,
                    p.GenericArguments.Select(a => new TypeReferenceModel(a)).ToArray()))
                    .ToArray()
                : null;
            this.genericArgumentModels = typeSyntax.IsGenericType == true
                ? typeSyntax.GenericArguments.Select(g => new TypeReferenceModel(g)).ToArray()
                : null;
            this.arrayRank = typeSyntax.IsArrayType == true
                ? typeSyntax.ArrayParameterRank
                : -1;

            // Set parent
            if (genericArgumentModels != null)
            {
                foreach (TypeReferenceModel genericArgument in genericArgumentModels)
                    genericArgument.parent = this;
            }
        }

        internal TypeReferenceModel(string typeName, string[] namespaceNames, ParentTypeReference[] parentTypes, TypeReferenceModel[] genericArguments = null, int arrayRank = -1, SyntaxSpan? span = null)
            : base(span)
        {
            this.typeName = new StringModel(typeName);
            this.namespaceName = namespaceNames != null
                ? new StringModel(string.Concat(namespaceNames))
                : null;
            this.parentTypes = parentTypes;
            this.genericArgumentModels = genericArguments;
            this.arrayRank = arrayRank;

            // Set parent
            if(genericArgumentModels != null)
            {
                foreach (TypeReferenceModel genericArgument in genericArgumentModels)
                    genericArgument.parent = this;
            }
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitTypeReference(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve generic arguments
            bool didResolveGenerics = false;
            if (genericArgumentModels != null)
            {
                for (int i = 0; i < genericArgumentModels.Length; i++)
                {
                    // Try to resolve the symbol
                    genericArgumentModels[i].ResolveSymbols(provider, report);

                    // Check for resolved
                    didResolveGenerics |= genericArgumentModels[i].IsResolved;
                }
            }

            // Try to resolve symbol
            if(IsGenericType == true && didResolveGenerics == true)
                this.typeSymbol = provider.ResolveTypeSymbol(ParentSymbol, typeName.Text, NamespaceNames, parentTypes, EvaluatedGenericTypeSymbols, arrayRank, Span);            

            // Check for generic argument usage
            if (typeSymbol != null)
            {
                // Check for generic type
                if (IsGenericType == true)
                {
                    // Check all generic arguments
                    CheckGenericArguments(typeSymbol, genericArgumentModels, report);
                }
            }
        }

        private void CheckGenericArguments(ITypeReferenceSymbol typeSymbol, TypeReferenceModel[] genericArgumentTypeSymbols, ICompileReportProvider report)
        {
            // Check for generic argument mismatch
            if(typeSymbol.GenericParameterSymbols == null)
            {
                report.ReportDiagnostic(Code.InvalidNoGenericArgument, MessageSeverity.Error, Span, typeSymbol);
                return;
            }

            // Check for mismatch generic argument count
            if(typeSymbol.GenericParameterSymbols.Length != genericArgumentTypeSymbols.Length)
            {
                report.ReportDiagnostic(Code.InvalidCountGenericArgument, MessageSeverity.Error, Span, typeSymbol, genericArgumentTypeSymbols.Length);
                return;
            }

            // Check all generic arguments
            for(int i = 0; i < genericArgumentTypeSymbols.Length; i++)
            {
                CheckGenericArgument(typeSymbol.GenericParameterSymbols[i], genericArgumentTypeSymbols[i], report);
            }
        }

        private void CheckGenericArgument(IGenericParameterIdentifierReferenceSymbol genericParameter, TypeReferenceModel genericArgument, ICompileReportProvider report)
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
                        report.ReportDiagnostic(Code.InvalidConstraintGenericArgument, MessageSeverity.Error, Span, genericArgument, genericConstraint);
                    }
                }
            }
        }
    }
}
