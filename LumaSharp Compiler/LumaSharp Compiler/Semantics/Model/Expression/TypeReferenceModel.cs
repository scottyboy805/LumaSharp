using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model.Expression
{
    public sealed class TypeReferenceModel : ExpressionModel
    {
        // Private
        private TypeReferenceSyntax syntax = null;
        private ITypeReferenceSymbol typeSymbol = null;

        // Properties
        public TypeReferenceSyntax Syntax
        {
            get { return syntax; }
        }

        public override bool IsStaticallyEvaluated
        {
            get { return false; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return typeSymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal TypeReferenceModel(SemanticModel model, SymbolModel parent, TypeReferenceSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitTypeReference(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Try to resolve symbol
            this.typeSymbol = provider.ResolveTypeSymbol(ParentSymbol, syntax);
        }
    }
}
