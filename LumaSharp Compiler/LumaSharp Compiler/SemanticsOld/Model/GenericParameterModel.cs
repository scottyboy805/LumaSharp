using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.SemanticsOld.Model
{
    internal sealed class GenericParameterModel : SymbolModel, IGenericParameterIdentifierReferenceSymbol
    {
        // Private
        private readonly StringModel identifier;
        private readonly TypeReferenceModel[] genericConstraintModels;
        private ITypeReferenceSymbol[] resolvedGenericConstraints = null;
        private ITypeReferenceSymbol resolvedAnyType = null;

        private IFieldReferenceSymbol[] fieldMembers = null;
        private IPropertyReferenceSymbol[] accessorMembers = null;
        private IMethodReferenceSymbol[] methodMembers = null;
        private IMethodReferenceSymbol[] operatorMembers = null;

        // Properties
        public string TypeName
        {
            get { return identifier.Text; }
        }

        public string[] NamespaceName
        {
            get { return null; }
        }

        public INamespaceReferenceSymbol NamespaceSymbol
        {
            get { return null; }
        }

        public ReservedType PrimitiveType
        {
            get { return ReservedType.Object; }
        }

        public bool IsPrimitive
        {
            get { return false; }
        }

        public bool IsClass
        {
            get { return true; }
        }

        public bool IsStruct
        {
            get { return false; }
        }

        public bool IsInterface
        {
            get { return false; }
        }

        public bool IsEnum
        {
            get { return false; }
        }

        public ITypeReferenceSymbol DeclaringTypeSymbol
        {
            get { return parent as ITypeReferenceSymbol; }
        }

        public IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols
        {
            get { return null; } // Cannot have generic parameter with geneirc parameters
        }

        public ITypeReferenceSymbol[] BaseTypeSymbols
        {
            get { return resolvedGenericConstraints; }
        }

        public ITypeReferenceSymbol[] TypeMemberSymbols
        {
            get { return null; } // No nested types allowed for generic parameter
        }

        public IFieldReferenceSymbol[] FieldMemberSymbols
        {
            get { return fieldMembers; }
        }

        public IPropertyReferenceSymbol[] AccessorMemberSymbols
        {
            get { return accessorMembers; }
        }

        public IMethodReferenceSymbol[] MethodMemberSymbols
        {
            get { return methodMembers; }
        }

        public IMethodReferenceSymbol[] OperatorMemberSymbols
        {
            get { return operatorMembers; }
        }

        public bool IsTypeParameter
        {
            get { return parent is ITypeReferenceSymbol; }
        }

        public bool IsMethodParameter
        {
            get { return parent is IMethodReferenceSymbol; }
        }

        public ITypeReferenceSymbol[] TypeConstraintSymbols
        {
            get { return resolvedGenericConstraints; }
        }

        public ITypeReferenceSymbol TypeSymbol
        {
            get
            {
                // The generic constraint members will be forwarded onto this type
                if (resolvedGenericConstraints != null && resolvedGenericConstraints.Length >= 1)
                    return this;

                // Type cannot be inferred or is not constrained, so default to any
                return resolvedAnyType;
            }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return Model.LibrarySymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                if(genericConstraintModels != null)
                {
                    foreach(TypeReferenceModel genericConstraint in genericConstraintModels)
                        yield return genericConstraint;
                }
            }
        }

        // Constructor
        public GenericParameterModel(GenericParameterSyntax genericParameterSyntax)
            : base(genericParameterSyntax != null ? genericParameterSyntax.GetSpan() : null)
        {
            // Check for null
            if(genericParameterSyntax == null)
                throw new ArgumentNullException(nameof(genericParameterSyntax));

            this.identifier = new StringModel(genericParameterSyntax.Identifier);
            this.genericConstraintModels = genericParameterSyntax.HasConstraints == true
                ? genericParameterSyntax.Constraints.Constraints.Select(c => new TypeReferenceModel(c)).ToArray() 
                : null;

            // Set parent
            if(genericConstraintModels != null)
            {
                foreach (TypeReferenceModel genericConstraintModel in genericConstraintModels)
                    genericConstraintModel.parent = this;
            }
        }

        public GenericParameterModel(string identifier, TypeReferenceModel[] genericConstraintTypes = null, SyntaxSpan? span = null)
            : base(span)
        {
            // Check for null or empty
            if(string.IsNullOrEmpty(identifier) == true)
                throw new ArgumentException(nameof(identifier) + " cannot be null or empty");

            this.identifier = new StringModel(identifier);
            this.genericConstraintModels = genericConstraintTypes;

            // Set parent
            if (genericConstraintModels != null)
            {
                foreach (TypeReferenceModel genericConstraintModel in genericConstraintModels)
                    genericConstraintModel.parent = this;
            }
        }


        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitGenericParameter(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, IDiagnosticReporter report)
        {
            // Resolve constraints
            if (genericConstraintModels != null)
            {
                // Resolve all constraints
                foreach(TypeReferenceModel genericConstraint in genericConstraintModels)
                {
                    genericConstraint.ResolveSymbols(provider, report);
                }

                // Get generic constraints that have been resolved
                resolvedGenericConstraints = genericConstraintModels
                    .Select(c => c.IsResolved == true ? c.EvaluatedTypeSymbol : null)
                    .ToArray();

                // Check constrains type usage
                CheckConstraintTypes(resolvedGenericConstraints, report);

                // Update members
                fieldMembers = resolvedGenericConstraints.Where(c => c.FieldMemberSymbols != null).SelectMany(c => c.FieldMemberSymbols).ToArray();
                accessorMembers = resolvedGenericConstraints.Where(c => c.AccessorMemberSymbols != null).SelectMany(c => c.AccessorMemberSymbols).ToArray();
                methodMembers = resolvedGenericConstraints.Where(c => c.MethodMemberSymbols != null).SelectMany(c => c.MethodMemberSymbols).ToArray();
                operatorMembers = resolvedGenericConstraints.Where(c => c.OperatorMemberSymbols != null).SelectMany(c => c.OperatorMemberSymbols).ToArray();
            }
            else
            {
                // Resolve any - treat the constraint type as any - Allows use of common methods like string
                resolvedAnyType = provider.ResolveTypeSymbol(ReservedType.Object, null, Span);
            }
        }

        private void CheckConstraintTypes(ITypeReferenceSymbol[] genericConstraints, IDiagnosticReporter report)
        {
            // Check all constraint types
            for(int i = 0; i < genericConstraints.Length; i++)
            {
                CheckConstraintType(genericConstraints[i], i, report);
            }
        }

        private void CheckConstraintType(ITypeReferenceSymbol genericConstraint, int index, IDiagnosticReporter report)
        {
            // Check for primitive
            if(genericConstraint.IsPrimitive == true)
            {
                report.Report(Code.InvalidPrimitiveGenericConstraint, DiagnosticSeverity.Error, Span, genericConstraint);
            }
        }
    }
}
