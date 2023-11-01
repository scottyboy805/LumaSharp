using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model.Expression;
using LumaSharp_Compiler.Semantics.Reference;

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
        public ReturnModel(SemanticModel model, SymbolModel parent, ReturnStatementSyntax syntax, int statementIndex)
            : base(model, parent, syntax, statementIndex)
        {
            this.syntax = syntax;
            this.returnModel = ExpressionModel.Any(model, this, syntax.ReturnExpression);
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitReturn(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Check if method has return
            IMethodReferenceSymbol parentMethod = ParentSymbol as IMethodReferenceSymbol;

            // Check for return
            if(HasReturnExpression == true)
            {
                // Resolve symbols in return model
                returnModel.ResolveSymbols(provider, report);

                if (returnModel.EvaluatedTypeSymbol != null && parentMethod.ReturnTypeSymbol != null)
                {
                    // Check for return type conversion
                    if (TypeChecker.IsTypeAssignable(returnModel.EvaluatedTypeSymbol, parentMethod.ReturnTypeSymbol) == false)
                    {
                        report.ReportMessage(Code.InvalidConversion, MessageSeverity.Error, syntax.StartToken.Source, returnModel.EvaluatedTypeSymbol, parentMethod.ReturnTypeSymbol);
                    }
                }
            }
        }
    }
}
