
using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class FieldModel : MemberModel, IFieldReferenceSymbol
    {
        // Private
        private FieldSyntax syntax = null;
        private TypeModel declaringType = null;
        private ITypeReferenceSymbol fieldTypeSymbol = null;

        // Properties
        public string FieldName
        {
            get { return syntax.Identifier.Text; }
        }

        public ITypeReferenceSymbol DeclaringTypeSymbol
        {
            get { return declaringType; }
        }

        public ITypeReferenceSymbol FieldTypeSymbol
        {
            get { return fieldTypeSymbol; }
        }

        // Constructor
        internal FieldModel(FieldSyntax syntax, SemanticModel model, TypeModel parent)
            : base(syntax, model, parent)
        {
            this.syntax = syntax;
            this.declaringType = parent;
        }

        // Methods
        public override bool ResolveSymbols(ISymbolProvider provider)
        {
            // Resolve field symbol
            fieldTypeSymbol = provider.ResolveTypeSymbol(syntax.FieldType);

            // Check for success
            return fieldTypeSymbol != null;
        }
    }
}
