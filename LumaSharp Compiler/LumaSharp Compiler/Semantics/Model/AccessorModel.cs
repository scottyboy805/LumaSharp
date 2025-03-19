using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
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
            get { return syntax.HasAccessModifiers == true && syntax.AccessModifiers.Any(m => m.Kind == SyntaxTokenKind.GlobalKeyword); }
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
            get { return syntax.AccessorBodies.Any(a => a.IsReadBody == true); }
        }

        public bool HasWriteBody
        {
            get { return syntax.AccessorBodies.Any(a => a.IsWriteBody == true); }
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
