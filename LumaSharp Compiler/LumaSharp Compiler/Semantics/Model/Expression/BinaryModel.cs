using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model.Expression
{
    public sealed class BinaryModel : ExpressionModel
    {
        // Private
        private BinaryExpressionSyntax syntax = null;
        private ExpressionModel left = null;
        private ExpressionModel right = null;
        private ITypeReferenceSymbol inferredTypeSymbol = null;
        private IMethodReferenceSymbol inferredMethodOperatorSymbol = null;

        // Properties
        public ExpressionModel Left
        {
            get { return left; }
        }

        public ExpressionModel Right
        {
            get { return right; }
        }

        public override bool IsStaticallyEvaluated
        {
            get { return left.IsStaticallyEvaluated == true && right.IsStaticallyEvaluated == true; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return inferredTypeSymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                yield return left;
                yield return right;
            }
        }

        // Constructor
        internal BinaryModel(SemanticModel model, SymbolModel parent, BinaryExpressionSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.left = Any(model, this, syntax.Left);
            this.right = Any(model, this, syntax.Right);
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve left
            left.ResolveSymbols(provider, report);

            // Resolve right
            right.ResolveSymbols(provider, report);
        }

        //private bool ResolveOperationOrOperatorMethod()
        //{
        //    // Get type codes
        //    PrimitiveType leftType = left.EvaluatedTypeSymbol.PrimitiveType;
        //    PrimitiveType rightType = right.EvaluatedTypeSymbol.PrimitiveType;

        //    switch (syntax.Operation.Text)
        //    {
        //        // Add operation
        //        case "+":
        //            {
        //                break;
        //            }
        //    }

        //}
    }
}
