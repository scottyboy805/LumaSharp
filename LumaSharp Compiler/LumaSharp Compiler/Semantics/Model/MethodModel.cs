using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class MethodModel : MemberModel, IMethodReferenceSymbol, IIdentifierReferenceSymbol
    {
        // Private
        private MethodSyntax syntax = null;
        private TypeModel declaringType = null;
        private ITypeReferenceSymbol returnTypeSymbol = null;
        private ITypeReferenceSymbol[] genericParameterSymbols = null;
        private ITypeReferenceSymbol[] parameterSymbols = null;

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

        public ITypeReferenceSymbol[] GenericParameterSymbols
        {
            get { return genericParameterSymbols; }
        }

        public ITypeReferenceSymbol[] ParameterSymbols
        {
            get { return parameterSymbols; }
        }

        // Constructor
        internal MethodModel(SemanticModel model, TypeModel parent, MethodSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.declaringType = parent;
        }

        // Methods
        public override bool ResolveSymbols(ISymbolProvider provider)
        {
            throw new NotImplementedException();
        }
    }
}
