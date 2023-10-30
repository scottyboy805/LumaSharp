using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.AST.Statement;

namespace LumaSharp_Compiler.Semantics.Model.Statement
{
    public abstract class StatementModel : SymbolModel
    {
        // Private
        private StatementSyntax syntax = null;
        private int statementIndex = 0;

        // Properties
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
        internal StatementModel(SemanticModel model, SymbolModel parent, StatementSyntax syntax, int statementIndex)
            : base(model, parent)
        {
            this.syntax = syntax;
            this.statementIndex = statementIndex;
        }

        // Methods
        internal static StatementModel Any(SemanticModel model, SymbolModel parent, StatementSyntax syntax, int statementIndex, IScopeModel scope = null)
        {
            // Check for variable
            if(syntax is VariableDeclarationStatementSyntax)
            {
                // Check for scope
                if (scope != null)
                    return scope.DeclareScopedLocal(model, syntax as VariableDeclarationStatementSyntax, statementIndex);

                // Create variable
                return new VariableModel(model, parent, syntax as VariableDeclarationStatementSyntax, statementIndex);
            }

            // Check for return
            if(syntax is ReturnStatementSyntax)
                return new ReturnModel(model, parent, syntax as ReturnStatementSyntax, statementIndex);

            // Check for assign
            if(syntax is AssignStatementSyntax)
                return new AssignModel(model, parent, syntax as AssignStatementSyntax, statementIndex);

            throw new NotSupportedException("Specified syntax is not supported");
        }
    }
}
