using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model.Expression;

namespace LumaSharp_Compiler.Semantics.Model.Statement
{
    public sealed class ReturnModel : StatementModel
    {
        // Private
        private ReturnStatementSyntax syntax = null;
        private ExpressionModel returnModel = null;

        // Properties
        public ExpressionModel ReturnModelExpression
        {
            get { return returnModel; }
        }

        public bool HasReturnExpression
        {
            get { return syntax.HasReturnExpression; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                if(returnModel != null)
                    yield return returnModel;
            }
        }

        // Constructor
        public ReturnModel(SemanticModel model, SymbolModel parent, ReturnStatementSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.returnModel = ExpressionModel.Any(model, this, syntax.ReturnExpression);
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Check for return
            if(HasReturnExpression == true)
            {
                // Resolve symbols in return model
                returnModel.ResolveSymbols(provider, report);
            }
        }
    }
}
