using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.SemanticsOld.Model
{
    public sealed class PropertyModel : MemberModel, IPropertyReferenceSymbol
    {
        // Private
        //private AccessorSyntax syntax = null;
        private TypeModel declaringType = null;
        private ITypeReferenceSymbol propertyTypeSymbol = null;

        // Properties
        public string AccessorName
        {
            get { return MemberName; }
        }

        public string IdentifierName
        {
            get { return MemberName; }
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
            get { return propertyTypeSymbol; }
        }
        public ITypeReferenceSymbol AccessorTypeSymbol
        {
            get { return propertyTypeSymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        public IMemberReferenceSymbol Getter => throw new NotImplementedException();

        public IMemberReferenceSymbol Setter => throw new NotImplementedException();

        // Constructor
        internal PropertyModel(SemanticModel model, TypeModel parent, AccessorSyntax syntax)
            : base(default, default, null)
        {
            this.declaringType = parent;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitAccessor(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve base
            base.ResolveSymbols(provider, report);

            // Resolve accessor symbol
            //propertyTypeSymbol = provider.ResolveTypeSymbol(declaringType, syntax.AccessorType);
        }

        //public override void StaticallyEvaluateMember(ISymbolProvider provider)
        //{
        //}
    }
}
