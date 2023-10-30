using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.AST.Statement;

namespace LumaSharp_Compiler.Semantics.Model.Statement
{
    public abstract class StatementModel : SymbolModel
    {
        // Private
        private StatementSyntax syntax = null;

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
        internal StatementModel(SemanticModel model, SymbolModel parent, StatementSyntax syntax)
            : base(model, parent)
        {
            this.syntax = syntax;
        }

        // Methods
        internal static StatementModel Any(SemanticModel model, SymbolModel parent, StatementSyntax syntax)
        {
            // Check for return
            if(syntax is ReturnStatementSyntax)
                return new ReturnModel(model, parent, syntax as ReturnStatementSyntax);

            // Check for assign
            if(syntax is AssignStatementSyntax)
                return new AssignModel(model, parent, syntax as AssignStatementSyntax);

            throw new NotSupportedException("Specified syntax is not supported");
        }
    }
}
