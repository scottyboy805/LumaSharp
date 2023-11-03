using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class GenericParameterModel : IGenericParameterIdentifierReferenceSymbol
    {
        // Private
        private GenericParameterSyntax syntax = null;
        private IReferenceSymbol parent = null;
        private ITypeReferenceSymbol[] genericConstraints = null;
        private ITypeReferenceSymbol anyType = null;

        private IFieldReferenceSymbol[] fieldMembers = null;
        private IAccessorReferenceSymbol[] accessorMembers = null;
        private IMethodReferenceSymbol[] methodMembers = null;
        private IMethodReferenceSymbol[] operatorMembers = null;

        // Properties
        public string TypeName
        {
            get { return syntax.Identifier.Text; }
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
            get { return null; }
        }

        public ITypeReferenceSymbol[] BaseTypeSymbols
        {
            get { return genericConstraints; }
        }

        public ITypeReferenceSymbol[] TypeMemberSymbols
        {
            get { return null; }
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

        public int Index
        {
            get { return syntax.Index; }
        }

        public IReferenceSymbol ParentSymbol
        {
            get { return parent; }
        }

        public ITypeReferenceSymbol[] TypeConstraintSymbols
        {
            get { return genericConstraints; }
        }

        public ITypeReferenceSymbol TypeSymbol
        {
            get
            {
                if(genericConstraints != null && genericConstraints.Length == 1)
                    return genericConstraints[0];

                // Type cannot be inferred
                return anyType;
            }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return parent.LibrarySymbol; }
        }

        public int SymbolToken
        {
            get { return -1; }
        }

        public string IdentifierName
        {
            get { return syntax.Identifier.Text; }
        }

        // Constructor
        public GenericParameterModel(GenericParameterSyntax syntax, IReferenceSymbol parent)
        {
            this.syntax = syntax;
            this.parent = parent;
        }

        // Methods
        public void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve any
            anyType = provider.ResolveTypeSymbol(PrimitiveType.Any, syntax.StartToken.Source);

            // Resolve constraints
            if(syntax.HasConstraintTypes == true)
            {
                // Get generic constraints
                genericConstraints = new ITypeReferenceSymbol[syntax.ConstraintTypeCount];

                // Resolve all
                for(int i = 0; i < genericConstraints.Length; i++)
                {
                    genericConstraints[i] = provider.ResolveTypeSymbol(parent, syntax.ConstraintTypes[i]);
                }

                // Update members
                fieldMembers = genericConstraints.SelectMany(c => c.FieldMemberSymbols).ToArray();
                accessorMembers = genericConstraints.SelectMany(c => c.AccessorMemberSymbols).ToArray();
                methodMembers = genericConstraints.SelectMany(c => c.MethodMemberSymbols).ToArray();
                operatorMembers = genericConstraints.SelectMany(c => c.OperatorMemberSymbols).ToArray();
            }
        }
    }
}
