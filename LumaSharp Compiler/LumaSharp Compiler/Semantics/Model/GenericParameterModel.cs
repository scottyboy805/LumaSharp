using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class GenericParameterModel : IGenericParameterIdentifierReferenceSymbol
    {
        // Private
        private GenericParameterSyntax syntax = null;
        private IReferenceSymbol parent = null;
        private ITypeReferenceSymbol[] genericConstraints = null;
        private ITypeReferenceSymbol anyType = null;

        // Properties
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
        public void ResolveSymbols(ISymbolProvider provider)
        {
            // Resolve any
            anyType = provider.ResolveTypeSymbol(PrimitiveType.Any);

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
            }
        }
    }
}
