using LumaSharp.Runtime;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Model
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

        private _TypeHandle typeHandle = default;

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
                if (genericConstraints != null && genericConstraints.Length == 1)
                    return genericConstraints[0];

                // Type cannot be inferred
                return anyType;
            }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return parent.LibrarySymbol; }
        }

        public _TokenHandle SymbolToken
        {
            get { return default; }
        }

        public _TypeHandle TypeHandle
        {
            get { return typeHandle; }
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
            if (syntax.HasConstraintTypes == true)
            {
                // Get generic constraints
                genericConstraints = new ITypeReferenceSymbol[syntax.ConstraintTypeCount];

                // Resolve all
                for (int i = 0; i < genericConstraints.Length; i++)
                {
                    genericConstraints[i] = provider.ResolveTypeSymbol(parent, syntax.ConstraintTypes[i]);
                }

                // Check constrains type usage
                CheckConstraintTypes(genericConstraints, report);

                // Update members
                fieldMembers = genericConstraints.Where(c => c.FieldMemberSymbols != null).SelectMany(c => c.FieldMemberSymbols).ToArray();
                accessorMembers = genericConstraints.Where(c => c.AccessorMemberSymbols != null).SelectMany(c => c.AccessorMemberSymbols).ToArray();
                methodMembers = genericConstraints.Where(c => c.MethodMemberSymbols != null).SelectMany(c => c.MethodMemberSymbols).ToArray();
                operatorMembers = genericConstraints.Where(c => c.OperatorMemberSymbols != null).SelectMany(c => c.OperatorMemberSymbols).ToArray();
            }
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
                report.ReportDiagnostic(Code.InvalidPrimitiveGenericConstraint, MessageSeverity.Error, syntax.ConstraintTypes[index].StartToken.Source, genericConstraint);
            }
        }
    }
}
