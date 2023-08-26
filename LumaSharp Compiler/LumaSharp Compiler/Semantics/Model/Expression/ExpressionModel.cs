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

        // Constructor
        internal ExpressionModel(SemanticModel model, ExpressionSyntax syntax)
            : base(model)
        {

        }

        // Methods
        internal static ExpressionModel Any(SemanticModel model, ExpressionSyntax syntax)
        {
            // Check for literal
            if(syntax is LiteralExpressionSyntax)
                return new ConstantModel(model, syntax as  LiteralExpressionSyntax);

            throw new NotSupportedException("Specified syntax is not supported");
        }
    }
}
