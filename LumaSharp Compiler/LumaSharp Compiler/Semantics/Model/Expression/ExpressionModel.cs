using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics.Model.Expression
{
    public abstract class ExpressionModel : SymbolModel
    {
        // Private
        private ExpressionSyntax syntax = null;

        // Properties
        public abstract bool IsStaticallyEvaluated { get; }

        public abstract ITypeReferenceSymbol EvaluatedTypeSymbol { get; }

        public IReferenceSymbol ParentSymbol
        {
            get
            {
                SymbolModel model = this;

                // Move up the hierarchy
                while ((model is IReferenceSymbol) == false && model.Parent != null)
                    model = model.Parent;

                // Get symbol
                return model as IReferenceSymbol;
            }
        }

        // Constructor
        internal ExpressionModel(SemanticModel model, SymbolModel parent, ExpressionSyntax syntax)
            : base(model, parent)
        {
        }

        // Methods
        internal static ExpressionModel Any(SemanticModel model, SymbolModel parent, ExpressionSyntax syntax)
        {
            // Check for literal
            if(syntax is LiteralExpressionSyntax)
                return new ConstantModel(model, parent, syntax as  LiteralExpressionSyntax);

            throw new NotSupportedException("Specified syntax is not supported");
        }
    }
}
