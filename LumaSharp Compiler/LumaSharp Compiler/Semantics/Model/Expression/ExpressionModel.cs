using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Semantics.Model.Expression
{
    public abstract class ExpressionModel : SymbolModel
    {
        // Private
        private ExpressionSyntax syntax = null;

        // Properties
        /// <summary>
        /// Can the expression value or type be determined at compile time.
        /// </summary>
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
            this.syntax = syntax;
        }

        // Methods
        internal static ExpressionModel Any(SemanticModel model, SymbolModel parent, ExpressionSyntax syntax)
        {
            // Check for literal
            if(syntax is LiteralExpressionSyntax)
                return new ConstantModel(model, parent, syntax as  LiteralExpressionSyntax);

            // Check for type
            if (syntax is TypeReferenceSyntax)
                return new TypeReferenceModel(model, parent, syntax as TypeReferenceSyntax);

            // Check for variable
            if(syntax is VariableReferenceExpressionSyntax)
                return new VariableReferenceModel(model, parent, syntax as VariableReferenceExpressionSyntax);

            // Check for field
            if(syntax is FieldAccessorReferenceExpressionSyntax)
                return new FieldAccessorReferenceModel(model, parent, syntax as FieldAccessorReferenceExpressionSyntax);

            // Binary
            if(syntax is BinaryExpressionSyntax)
                return new BinaryModel(model, parent, syntax as BinaryExpressionSyntax);

            // This
            if (syntax is ThisExpressionSyntax)
                return new ThisReferenceModel(model, parent, syntax as ThisExpressionSyntax);

            throw new NotSupportedException("Specified syntax is not supported");
        }
    }
}
