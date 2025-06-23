using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class TypeofModel : ExpressionModel
    {
        // Private
        private readonly TypeReferenceModel typeModel = null;

        // Properties
        public override bool IsStaticallyEvaluated
        {
            get { return false; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return typeModel.EvaluatedTypeSymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield return typeModel; }
        }

        // Constructor
        public TypeofModel(TypeofExpressionSyntax typeofSyntax)
            : base(typeofSyntax != null ? typeofSyntax.GetSpan() : null)
        {
            // Check for null
            if (typeofSyntax == null)
                throw new ArgumentNullException(nameof(typeofSyntax));

            this.typeModel = new TypeReferenceModel(typeofSyntax.TypeReference);

            // Set parent
            this.typeModel.parent = this;
        }

        public TypeofModel(TypeReferenceModel type, SyntaxSpan? span)
            : base(span)
        {
            // Check for null
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            this.typeModel = type;

            // Set parent
            this.typeModel.parent = this;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitTypeToken(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve type
            typeModel.ResolveSymbols(provider, report);
        }
    }
}
