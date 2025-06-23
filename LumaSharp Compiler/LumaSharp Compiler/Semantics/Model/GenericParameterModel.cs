using LumaSharp.Runtime;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class GenericParameterModel : SymbolModel, IGenericParameterIdentifierReferenceSymbol
    {
        // Private
        private readonly StringModel identifier;
        private readonly TypeReferenceModel[] genericConstraintModels;
        private ITypeReferenceSymbol[] resolvedGenericConstraints = null;
        private ITypeReferenceSymbol resolvedAnyType = null;

        private IFieldReferenceSymbol[] fieldMembers = null;
        private IAccessorReferenceSymbol[] accessorMembers = null;
        private IMethodReferenceSymbol[] methodMembers = null;
        private IMethodReferenceSymbol[] operatorMembers = null;

        private _TokenHandle token = default;
        private _TypeHandle typeHandle = default;

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

        public PrimitiveType PrimitiveType
        {
            get { return PrimitiveType.Any; }
        }

        public bool IsPrimitive
        {
            get { return false; }
        }

        public bool IsType
        {
            get { return true; }
        }

        public bool IsContract
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

        public IAccessorReferenceSymbol[] AccessorMemberSymbols
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

        public _TokenHandle Token
        {
            get { return token; }
        }

        public _TypeHandle TypeHandle
        {
            get { return typeHandle; }
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

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
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
                resolvedAnyType = provider.ResolveTypeSymbol(PrimitiveType.Any, null, Span);
            }

            // Get the token
            this.token = GetGenericParameterToken();
        }

        private void CheckConstraintTypes(ITypeReferenceSymbol[] genericConstraints, ICompileReportProvider report)
        {
            // Check all constraint types
            for(int i = 0; i < genericConstraints.Length; i++)
            {
                CheckConstraintType(genericConstraints[i], i, report);
            }
        }

        private void CheckConstraintType(ITypeReferenceSymbol genericConstraint, int index, ICompileReportProvider report)
        {
            // Check for primitive
            if(genericConstraint.IsPrimitive == true)
            {
                report.ReportDiagnostic(Code.InvalidPrimitiveGenericConstraint, MessageSeverity.Error, Span, genericConstraint);
            }
        }

        private _TokenHandle GetGenericParameterToken()
        {
            // Check parent type
            if(parent is TypeModel parentType)
            {
                // Get the index of this parameter
                int index = Array.IndexOf(parentType.GenericParameterSymbols, this);

                // Check for invalid index
                if (index == -1)
                    throw new Exception("Generic parameter could not be indexed in parent: " + parent);

                // Create the token
                return new _TokenHandle(TokenKind.GenericTypeParameter, index);
            }
            // Check parent method
            else if(parent is MethodModel parentMethod)
            {
                // Get the index of this parameter
                int index = Array.IndexOf(parentMethod.GenericParameterSymbols, this);

                // Check for invalid index
                if (index == -1)
                    throw new Exception("Generic parameter could not be indexed in parent: " + parent);

                // Create the token
                return new _TokenHandle(TokenKind.GenericMethodParameter, index);
            }
            else
            {
                throw new Exception("Cannot determine context of generic parameter: " + parent);
            }
        }
    }
}
