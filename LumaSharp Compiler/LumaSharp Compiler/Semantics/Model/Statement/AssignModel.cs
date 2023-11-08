using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.AST.Statement;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model.Expression;
using LumaSharp_Compiler.Semantics.Reference;

namespace LumaSharp_Compiler.Semantics.Model.Statement
{
    public enum AssignOperation
    {
        Assign,
        AddAssign,
        SubtractAssign,
        MultiplyAssign,
        DivideAssign,
    }

    public sealed class AssignModel : StatementModel
    {
        // Private
        //private AssignStatementSyntax syntax = null;
        private SyntaxToken operationToken = null;
        private ExpressionModel left = null;
        private ExpressionModel right = null;
        private AssignOperation operation = 0;

        // Properties
        public ExpressionModel Left
        {
            get { return left; }
        }

        public ExpressionModel Right
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
                yield return left;
                yield return right;
            }
        }

        // Constructor
        public AssignModel(SemanticModel model, SymbolModel parent, AssignStatementSyntax syntax, int statementIndex)
            : base(model, parent, syntax, statementIndex)
        {
            //this.syntax = syntax;
            this.operationToken = syntax.AssignOperation;
            this.left = ExpressionModel.Any(model, this, syntax.Left);
            this.right = ExpressionModel.Any(model, this, syntax.Right);
        }

        public AssignModel(SemanticModel model, SymbolModel parent, VariableDeclarationStatementSyntax syntax, ExpressionModel left, ExpressionModel right, int statementIndex)
            : base(model, parent, syntax, statementIndex)
        {
            this.operationToken = SyntaxToken.Assign();
            this.left = left;
            this.right = right;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitAssign(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve left
            left.ResolveSymbols(provider, report);

            // Resolve right
            right.ResolveSymbols(provider, report);

            // Check for resolved
            if (left.EvaluatedTypeSymbol != null && right.EvaluatedTypeSymbol != null)
            {
                // Check for assignment
                if (TypeChecker.IsTypeAssignable(right.EvaluatedTypeSymbol, left.EvaluatedTypeSymbol) == false)
                {
                    report.ReportMessage(Code.InvalidConversion, MessageSeverity.Error, right.Source, right.EvaluatedTypeSymbol, left.EvaluatedTypeSymbol);
                }
            }


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
