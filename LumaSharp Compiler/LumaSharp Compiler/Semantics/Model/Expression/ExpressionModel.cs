using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    public abstract class ExpressionModel : SymbolModel
    {
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
        internal ExpressionModel(SyntaxSpan? span)
            : base(span)
        {
        }

        // Methods
        //public virtual ExpressionModel StaticallyEvaluateExpression(ISymbolProvider provider)
        //{
        //    return this;
        //}

        //public virtual object GetStaticallyEvaluatedValue()
        //{
        //    return null;
        //}

        internal static ExpressionModel Any(ExpressionSyntax syntax, SymbolModel parent)
        {
            ExpressionModel model = null;

            // Check for literal
            if (syntax is LiteralExpressionSyntax literal)
            {
                model = new ConstantModel(literal);
            }
            // Check for type
            else if (syntax is TypeReferenceSyntax typeReference)
            {
                model = new TypeReferenceModel(typeReference);
            }
            // Check for variable
            else if (syntax is VariableReferenceExpressionSyntax variableReference)
            {
                model = new VariableReferenceModel(variableReference);
            }

            // Check for variable assign
            //if(syntax is VariableAssignExpressionSyntax)
            //    return new AssignModel(model, parent, syntax as )

            // Check for field
            else if (syntax is MemberAccessExpressionSyntax memberAccess)
            {
                model = new FieldAccessorReferenceModel(memberAccess);
            }
            // Check for method
            else if (syntax is MethodInvokeExpressionSyntax methodInvoke)
            {
                model = new MethodInvokeModel(methodInvoke);
            }
            // Binary
            else if (syntax is BinaryExpressionSyntax binary)
            {
                model = new BinaryModel(binary);
            }
            // This
            else if (syntax is ThisExpressionSyntax _this)
            {
                model = new ThisModel(_this);
            }
            // New
            else if (syntax is NewExpressionSyntax _new)
            {
                model = new NewModel(_new);
            }

            // Check for null
            if(model == null)
                throw new NotSupportedException("Specified syntax is not supported: " + syntax.GetType());

            // Set parent
            model.parent = parent;
            return model;
        }
    }
}
