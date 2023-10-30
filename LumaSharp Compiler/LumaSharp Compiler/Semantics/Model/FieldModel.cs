
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class FieldModel : MemberModel, IFieldReferenceSymbol, IIdentifierReferenceSymbol
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
            get { return fieldTypeSymbol; }
        }

        public ITypeReferenceSymbol FieldTypeSymbol
        {
            get { return fieldTypeSymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal FieldModel(SemanticModel model, TypeModel parent, FieldSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.declaringType = parent;
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve field symbol
            fieldTypeSymbol = provider.ResolveTypeSymbol(declaringType, syntax.FieldType);
        }
    }
}
