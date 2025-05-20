using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Reference;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class AssignModel : StatementModel
    {
        // Private
        //private AssignStatementSyntax syntax = null;
        private readonly SyntaxToken operationToken;
        private readonly ExpressionModel[] left;
        private readonly ExpressionModel[] right;
        private AssignOperation operation;

        // Properties
        public ExpressionModel[] Left
        {
            get { return left; }
        }

        public ExpressionModel[] Right
        {
            get { return right; }
        }

        public AssignOperation Operation
        {
            get { return operation; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                foreach (ExpressionModel expression in left)
                    yield return expression;

                foreach(ExpressionModel expression in right)
                    yield return expression;
            }
        }

        // Constructor
        public AssignModel(SemanticModel model, SymbolModel parent, AssignStatementSyntax syntax, int statementIndex)
            : base(model, parent, syntax, statementIndex)
        {
            //this.syntax = syntax;
            this.operationToken = syntax.AssignExpression.Assign;
            this.left = syntax.Left.Select(l => ExpressionModel.Any(model, this, l)).ToArray();
            this.right = syntax.Right.Select(r => ExpressionModel.Any(model, this, r)).ToArray();
        }

        public AssignModel(SemanticModel model, SymbolModel parent, VariableDeclarationStatementSyntax syntax, ExpressionModel left, ExpressionModel right, int statementIndex)
            : base(model, parent, syntax, statementIndex)
        {
            this.operationToken = syntax.Assignment.Assign;
            this.left = new ExpressionModel[] { left };
            this.right = new ExpressionModel[] { right };
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitAssign(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve left
            foreach(ExpressionModel leftModel in left)
                leftModel.ResolveSymbols(provider, report);

            // Resolve right
            foreach(ExpressionModel rightModel in right)
                rightModel.ResolveSymbols(provider, report);


            // Check for assignable
            if(left.Length == right.Length)
            {
                for(int i = 0; i < left.Length; i++)
                {
                    // Check for resolved
                    if (left[i].EvaluatedTypeSymbol != null && right[i].EvaluatedTypeSymbol != null)
                    {
                        // Check for assignment
                        if (TypeChecker.IsTypeAssignable(right[i].EvaluatedTypeSymbol, left[i].EvaluatedTypeSymbol) == false)
                        {
                            report.ReportDiagnostic(Code.InvalidConversion, MessageSeverity.Error, right[i].Source, right[i].EvaluatedTypeSymbol, left[i].EvaluatedTypeSymbol);
                        }
                    }
                }
            }
            // Check for method
            else if(right.Length == 1 && right[0] is MethodInvokeModel invokeMethod)
            {
                // Check return types count
                if(left.Length == invokeMethod.MethodIdentifier.ReturnTypeSymbols.Length)
                {
                    // Get the method symbol
                    ITypeReferenceSymbol[] methodReturnSymbols = invokeMethod.MethodIdentifier.ReturnTypeSymbols;

                    for (int i = 0; i < left.Length; i++)
                    {
                        // Check for resolved
                        if (left[i].EvaluatedTypeSymbol != null && methodReturnSymbols[i] != null)
                        {
                            // Check for assignment
                            if (TypeChecker.IsTypeAssignable(methodReturnSymbols[i], left[i].EvaluatedTypeSymbol) == false)
                            {
                                report.ReportDiagnostic(Code.InvalidConversion, MessageSeverity.Error, invokeMethod.Source, methodReturnSymbols[i], left[i].EvaluatedTypeSymbol);
                            }
                        }
                    }
                }
                else
                {
                    // Method does not return x amount of parameters
                }
            }
            // Invalid assignment count
            else
            {

            }

            // Check for resolved
            //if (left.EvaluatedTypeSymbol != null && right.EvaluatedTypeSymbol != null)
            //{
            //    // Check for assignment
            //    if (TypeChecker.IsTypeAssignable(right.EvaluatedTypeSymbol, left.EvaluatedTypeSymbol) == false)
            //    {
            //        report.ReportMessage(Code.InvalidConversion, MessageSeverity.Error, right.Source, right.EvaluatedTypeSymbol, left.EvaluatedTypeSymbol);
            //    }
            //}


            // Parse operation
            switch(operationToken.Text)
            {
                case "=":
                    {
                        operation = AssignOperation.Assign;
                        break;
                    }
                case "+=": 
                    {
                        operation = AssignOperation.AddAssign;
                        break;
                    }
                case "-=":
                    {
                        operation = AssignOperation.SubtractAssign;
                        break;
                    }
                case "*=":
                    {
                        operation = AssignOperation.MultiplyAssign;
                        break;
                    }
                case "/=":
                    {
                        operation = AssignOperation.DivideAssign;
                        break;
                    }
            }
        }
    }
}
