using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class MethodModel : MemberModel, IMethodReferenceSymbol, IIdentifierReferenceSymbol
    {
        // Private
        private MethodSyntax syntax = null;
        private TypeModel declaringType = null;
        private ITypeReferenceSymbol returnTypeSymbol = null;
        private IGenericParameterIdentifierReferenceSymbol[] genericParameterIdentifierSymbols = null;
        private ILocalIdentifierReferenceSymbol[] parameterIdentifierSymbols = null;

        // Properties
        public string MethodName
        {
            get { return syntax.Identifier.Text; }
        }

        public string IdentifierName
        {
            get { return syntax.Identifier.Text; }
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
            get { return returnTypeSymbol; }
        }

        public ITypeReferenceSymbol ReturnTypeSymbol
        {
            get { return returnTypeSymbol; }
        }

        public IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols
        {
            get { return genericParameterIdentifierSymbols; }
        }

        public ILocalIdentifierReferenceSymbol[] ParameterSymbols
        {
            get { return parameterIdentifierSymbols; }
        }

        public bool HasReturnType
        {
            get { return syntax.ReturnType.Identifier.Text != "void"; }
        }

        public bool HasGenericParameters
        {
            get { return syntax.HasGenericParameters; }
        }

        public bool HasBody
        {
            get { return syntax.HasBody; }
        }

        // Constructor
        internal MethodModel(SemanticModel model, TypeModel parent, MethodSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.declaringType = parent;
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider)
        {
            // Get return type
            returnTypeSymbol = provider.ResolveTypeSymbol(declaringType, syntax.ReturnType);

            // Resolve generics
            if(syntax.HasGenericParameters == true)
            {
                // Create symbol array
                genericParameterIdentifierSymbols = new IGenericParameterIdentifierReferenceSymbol[syntax.GenericParameters.GenericParameterCount];

                // Resolve all
                for(int i = 0; i < genericParameterIdentifierSymbols.Length; i++)
                {
                    genericParameterIdentifierSymbols[i] = new GenericParameterModel(syntax.GenericParameters.GenericParameters[i], this);
                }
            }

            // Resolve parameters
            if(syntax.HasParameters == true)
            {
                // Create parameter array
                parameterIdentifierSymbols = new ILocalIdentifierReferenceSymbol[syntax.Parameters.ParameterCount];

                // Resolve all
                for(int i = 0; i < parameterIdentifierSymbols.Length; i++)
                {
                    parameterIdentifierSymbols[i] = new LocalOrParameterModel(syntax.Parameters.Parameters[i], this, i);
                }
            }
        }
    }
}
