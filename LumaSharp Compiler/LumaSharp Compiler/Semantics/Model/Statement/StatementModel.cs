using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    public abstract class StatementModel : SymbolModel
    {
        // Properties
        public IReferenceSymbol ParentSymbol
        {
            get
            {
                SymbolModel model = this;

                // Move up the hierarchy
                while (model == this || ((model is IReferenceSymbol) == false && model.Parent != null))
                    model = model.Parent;

                // Get symbol
                return model as IReferenceSymbol;
            }
        }

        // Constructor
        internal StatementModel(SyntaxSpan? span)
            : base(span)
        {
        }

        // Methods
        public virtual void StaticallyEvaluateStatement(ISymbolProvider provider) { }

        protected T GetParentSymbol<T>() where T : class, IReferenceSymbol
        {
            SymbolModel model = this;

            // Move up the hierarchy
            while (model == this || ((model is T) == false && model.Parent != null))
                model = model.Parent;

            // Get symbol
            return model as T;
        }

        internal static StatementModel Any(StatementSyntax syntax, SymbolModel parent)
        {
            StatementModel model = null;

            // Check for variable
            if (syntax is VariableDeclarationStatementSyntax variable)
            {
                // Create variable
                model = new VariableModel(variable);
            }
            // Check for return
            else if (syntax is ReturnStatementSyntax _return)
            {
                model = new ReturnModel(_return);
            }
            // Check for assign
            else if (syntax is AssignStatementSyntax assign)
            {
                model = new AssignModel(assign);
            }
            // Check for condition
            else if (syntax is ConditionStatementSyntax condition)
            {
                model = new ConditionModel(condition);
            }
            // Check for for loop
            else if (syntax is ForStatementSyntax _for)
            {
                model = new ForModel(_for);
            }
            // Check for method invoke
            else if (syntax is MethodInvokeStatementSyntax methodInvoke)
            {
                model = new MethodInlineInvokeModel(methodInvoke);
            }

            // Check for null
            if(model == null)
                throw new NotSupportedException("Specified syntax is not supported");

            model.parent = parent;
            return model;
        }
    }
}
