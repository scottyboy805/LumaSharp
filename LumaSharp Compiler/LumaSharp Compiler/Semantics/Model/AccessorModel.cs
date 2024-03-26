using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

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

        public bool IsGlobal
        {
            get { return syntax.HasAccessModifiers == true && syntax.AccessModifiers.FirstOrDefault(m => m.Text == "global") != null; }
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

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal AccessorModel(SemanticModel model, TypeModel parent, AccessorSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.declaringType = parent;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitAccessor(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Get symbol token
            memberToken = provider.GetDeclaredSymbolToken(this);

            // Resolve accessor symbol
            accessorTypeSymbol = provider.ResolveTypeSymbol(declaringType, syntax.AccessorType);
        }

        public override void StaticallyEvaluateMember(ISymbolProvider provider)
        {
        }
    }
}
