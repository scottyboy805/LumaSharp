using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Reference;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class ReturnModel : StatementModel
    {
        // Private
        private readonly ExpressionModel[] returnModels;

        // Properties
        public ExpressionModel[] ReturnModels
        {
            get { return returnModels; }
        }

        public bool HasReturnValue
        {
            get { return returnModels != null; }
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
        public ReturnModel(ReturnStatementSyntax returnSyntax)
            : base(returnSyntax != null ? returnSyntax.GetSpan() : null)
        {
            // Check for null
            if(returnSyntax == null)
                throw new ArgumentNullException(nameof(returnSyntax));

            this.returnModels = returnSyntax.HasExpressions == true
                ? returnSyntax.Expressions.Select(e => ExpressionModel.Any(e, this)).ToArray()
                : null;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitReturn(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Check if method has return
            IMethodReferenceSymbol parentMethod = GetParentSymbol<IMethodReferenceSymbol>();

            // Check for return
            if(HasReturnValue == true)
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
                            report.ReportDiagnostic(Code.InvalidConversion, MessageSeverity.Error, returnModels[i].Span, returnModels[i].EvaluatedTypeSymbol, parentMethod.ReturnTypeSymbols[i]);
                        }
                    }
                }
            }
        }

        public override void StaticallyEvaluateStatement(ISymbolProvider provider)
        {
            // Check for expression which can be statically evaluated
            if(HasReturnValue == true)
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
