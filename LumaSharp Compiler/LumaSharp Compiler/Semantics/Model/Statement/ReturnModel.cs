using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Reference;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class ReturnModel : StatementModel
    {
        // Private
        private ReturnStatementSyntax syntax = null;
        private ExpressionModel[] returnModels = null;

        // Properties
        public ExpressionModel[] ReturnModelExpressions
        {
            get { return returnModels; }
        }

        public bool HasReturnExpression
        {
            get { return syntax.HasExpressions; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                if (returnModels != null)
                {
                    foreach(ExpressionModel model in returnModels)
                        yield return model;
                }
            }
        }

        // Constructor
        public ReturnModel(SemanticModel model, SymbolModel parent, ReturnStatementSyntax syntax, int statementIndex)
            : base(model, parent, syntax, statementIndex)
        {
            this.syntax = syntax;

            if(syntax.Expressions != null)
                this.returnModels = syntax.Expressions.Select(e => ExpressionModel.Any(model, this, e)).ToArray();
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitReturn(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Check if method has return
            IMethodReferenceSymbol parentMethod = GetParentSymbol<IMethodReferenceSymbol>(); //ParentSymbol as IMethodReferenceSymbol;

            // Check for return
            if(HasReturnExpression == true)
            {
                // Check for incorrect return count
                if(returnModels.Length != parentMethod.ReturnTypeSymbols.Length)
                {
                    // TODO - report error
                }

                // Process all return models
                for(int i = 0; i < returnModels.Length; i++)
                {
                    // Resolve symbols in return model
                    returnModels[i].ResolveSymbols(provider, report);

                    if (returnModels[i].EvaluatedTypeSymbol != null && parentMethod.ReturnTypeSymbols[i] != null && i < returnModels.Length && i < parentMethod.ReturnTypeSymbols.Length)
                    {
                        // Check for return type conversion
                        if (TypeChecker.IsTypeAssignable(returnModels[i].EvaluatedTypeSymbol, parentMethod.ReturnTypeSymbols[i]) == false)
                        {
                            report.ReportDiagnostic(Code.InvalidConversion, MessageSeverity.Error, syntax.StartToken.Source, returnModels[i].EvaluatedTypeSymbol, parentMethod.ReturnTypeSymbols[i]);
                        }
                    }
                }
            }
        }

        public override void StaticallyEvaluateStatement(ISymbolProvider provider)
        {
            // Check for expression which can be statically evaluated
            if(HasReturnExpression == true)
            {
                // Process all models
                for(int i = 0; i < returnModels.Length; i++)
                {
                    // Check for statically evaluated
                    if (returnModels[i].IsStaticallyEvaluated == false)
                        continue;

                    // Evaluate the expression
                    returnModels[i] = returnModels[i].StaticallyEvaluateExpression(provider);
                }
            }
        }
    }
}
