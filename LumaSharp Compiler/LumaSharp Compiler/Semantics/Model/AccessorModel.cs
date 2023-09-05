
using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class AccessorModel : MemberModel, IAccessorReferenceSymbol, IIdentifierReferenceSymbol
    {
        // Private
        private AccessorSyntax syntax = null;
        private TypeModel declaringType = null;
        private ITypeReferenceSymbol accessorTypeSymbol = null;

        // Properties
        public string AccessorName
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
            get { return accessorTypeSymbol; }
        }
        public ITypeReferenceSymbol AccessorTypeSymbol
        {
            get { return accessorTypeSymbol; }
        }

        public bool HasReadBody
        {
            get { return syntax.HasReadBody; }
        }

        public bool HasWriteBody
        {
            get { return syntax.HasWriteBody; }
        }

        // Constructor
        internal AccessorModel(SemanticModel model, TypeModel parent, AccessorSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.declaringType = parent;
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider)
        {
            // Resolve accessor symbol
            accessorTypeSymbol = provider.ResolveTypeSymbol(declaringType, syntax.AccessorType);
        }
    }
}
