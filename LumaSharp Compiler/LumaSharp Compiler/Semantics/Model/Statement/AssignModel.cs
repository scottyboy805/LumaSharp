using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Reference;

namespace LumaSharp.Compiler.Semantics.Model
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
        public AssignModel(AssignStatementSyntax assignSyntax)
            : base(assignSyntax != null ? assignSyntax.GetSpan() : null)
        {
            // Check for null
            if(assignSyntax == null)
                throw new ArgumentNullException(nameof(assignSyntax));

            this.left = assignSyntax.Left.Select(e => ExpressionModel.Any(e, this)).ToArray();
            this.operationToken = assignSyntax.AssignExpression.Assign;
            this.right = assignSyntax.Right.Select(e => ExpressionModel.Any(e, this)).ToArray();

            // Set parent
            foreach (ExpressionModel l in left)
                l.parent = this;

            foreach (ExpressionModel r in right)
                r.parent = this;
        }

        public AssignModel(ExpressionModel[] left, AssignOperation operation, ExpressionModel[] right, SyntaxSpan? span)
            : base(span)
        {
            // Check for null
            if(left == null)
                throw new ArgumentNullException(nameof(left));

            if(right == null)
                throw new ArgumentNullException(nameof(right));

            this.left = left;
            this.operationToken = CreateAssignOperation(operation);
            this.right = right;

            // Set parent
            foreach (ExpressionModel l in left)
                l.parent = this;

            foreach (ExpressionModel r in right)
                r.parent = this;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitAssign(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Try to get assign operation
            try
            {
                operation = GetAssignOperation(operationToken);
            }
            catch(Exception)
            {
                report.ReportDiagnostic(Code.InvalidOperation, MessageSeverity.Error, operationToken.Span);
                return;
            }

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
                            report.ReportDiagnostic(Code.InvalidConversion, MessageSeverity.Error, right[i].Span, right[i].EvaluatedTypeSymbol, left[i].EvaluatedTypeSymbol);
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
                                report.ReportDiagnostic(Code.InvalidConversion, MessageSeverity.Error, invokeMethod.Span, methodReturnSymbols[i], left[i].EvaluatedTypeSymbol);
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
        }

        private static AssignOperation GetAssignOperation(SyntaxToken op)
        {
            return op.Kind switch
            {
                SyntaxTokenKind.AssignSymbol => AssignOperation.Assign,
                SyntaxTokenKind.AssignPlusSymbol => AssignOperation.AddAssign,
                SyntaxTokenKind.AssignMinusSymbol => AssignOperation.SubtractAssign,
                SyntaxTokenKind.AssignMultiplySymbol => AssignOperation.MultiplyAssign,
                SyntaxTokenKind.AssignDivideSymbol => AssignOperation.DivideAssign,

                _ => throw new InvalidOperationException("Invalid assign operator: " + op.Kind)
            };
        }

        private static SyntaxToken CreateAssignOperation(AssignOperation op)
        {
            return Syntax.Token(op switch
            {
                AssignOperation.Assign => SyntaxTokenKind.AssignSymbol,
                AssignOperation.AddAssign => SyntaxTokenKind.AssignPlusSymbol,
                AssignOperation.SubtractAssign => SyntaxTokenKind.AssignMinusSymbol,
                AssignOperation.MultiplyAssign => SyntaxTokenKind.AssignMultiplySymbol,
                AssignOperation.DivideAssign => SyntaxTokenKind.AssignDivideSymbol,

                _ => throw new NotSupportedException(op.ToString())
            });
        }
    }
}
