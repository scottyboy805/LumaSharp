using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class SizeofModel : ExpressionModel
    {
        // Private
        private readonly TypeReferenceModel typeModel = null;
        private ITypeReferenceSymbol returnSymbol = null;

        // Properties
        public override bool IsStaticallyEvaluated
        {
            get { return false; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return returnSymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield return typeModel; }
        }

        // Constructor
        public SizeofModel(SizeofExpressionSyntax sizeofSyntax)
            : base(sizeofSyntax != null ? sizeofSyntax.GetSpan() : null)
        {
            // Check for null
            if (sizeofSyntax == null)
                throw new ArgumentNullException(nameof(sizeofSyntax));

            this.typeModel = new TypeReferenceModel(sizeofSyntax.TypeReference);

            // Set parent
            this.typeModel.parent = this;
        }

        public SizeofModel(TypeReferenceModel type, SyntaxSpan? span)
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
            visitor.VisitSize(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve type
            typeModel.ResolveSymbols(provider, report);

            // Resolve return type
            returnSymbol = provider.ResolveTypeSymbol(PrimitiveType.I32, null, Span);
        }
    }
}
