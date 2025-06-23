using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Reference;

namespace LumaSharp.Compiler.Semantics.Model
{
    public enum BinaryOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulus,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,
        Equal,
        NotEqual,
        And,
        Or
    }

    public sealed class BinaryModel : ExpressionModel
    {
        // Private
        private readonly ExpressionModel left;
        private readonly ExpressionModel right;
        private readonly SyntaxToken operationToken;
        private BinaryOperation operation = 0;
        private ITypeReferenceSymbol inferredTypeSymbol = null;
        private IMethodReferenceSymbol inferredMethodOperatorSymbol = null;        

        // Properties
        public ExpressionModel Left
        {
            get { return left; }
        }

        public ExpressionModel Right
        {
            get { return right; }
        }

        public bool IsLeftPromotionRequired
        {
            get { return left.EvaluatedTypeSymbol != null && IsSmallPrimitive(left.EvaluatedTypeSymbol); }
        }

        public bool IsRightPromotionRequired
        {
            get { return right.EvaluatedTypeSymbol != null && IsSmallPrimitive(right.EvaluatedTypeSymbol); }
        }

        public override bool IsStaticallyEvaluated
        {
            get { return left.IsStaticallyEvaluated == true && right.IsStaticallyEvaluated == true; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get 
            {
                // Check for operator method available
                if (inferredMethodOperatorSymbol != null)
                    return inferredMethodOperatorSymbol.ReturnTypeSymbols[0];

                return inferredTypeSymbol; 
            }
        }

        public BinaryOperation Operation
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
        internal BinaryModel(BinaryExpressionSyntax binarySyntax)
            : base(binarySyntax != null ? binarySyntax.GetSpan() : default)
        {
            // Check null
            if (binarySyntax == null)
                throw new ArgumentNullException(nameof(binarySyntax));

            // Create model
            this.left = ExpressionModel.Any(binarySyntax.Left, this);
            this.operationToken = binarySyntax.Operation;
            this.right = ExpressionModel.Any(binarySyntax.Right, this);
        }

        internal BinaryModel(ExpressionModel left, BinaryOperation operation, ExpressionModel right, SyntaxSpan span)
            : base(span)
        {
            // Check null
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            this.left = left;
            this.operationToken = CreateBinaryOperation(operation); // Used to give better error reporting and easier creation
            this.right = right;

            // Set parent
            left.parent = this;
            right.parent = this;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitBinary(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve left
            left.ResolveSymbols(provider, report);

            // Resolve right
            right.ResolveSymbols(provider, report);


            //Try to get the target operation
            try
            {
                operation = GetBinaryOperation(operationToken);
            }
            catch (NotSupportedException)
            {
                report.ReportDiagnostic(Code.InvalidOperation, MessageSeverity.Error, operationToken.Span);
                return;
            }

            // Check for symbols resolved
            if (left.EvaluatedTypeSymbol != null && right.EvaluatedTypeSymbol != null)
            {
                // Get left/right types
                PrimitiveType leftTypeCode = GetPromotedTypeCode(left.EvaluatedTypeSymbol);
                PrimitiveType rightTypeCode = GetPromotedTypeCode(right.EvaluatedTypeSymbol);

                // Check for both primitive
                if (leftTypeCode != PrimitiveType.Any && rightTypeCode != PrimitiveType.Any)
                {
                    PrimitiveType opReturnType = OpTable.GetOperationReturnType(leftTypeCode, rightTypeCode);

                    //// Check for operation defined
                    //switch (operation)
                    //{
                    //    case BinaryOperation.Add:
                    //        {
                    //            // Check for defined
                    //            opReturnType = OpTable.GetAddOperationReturnType(leftTypeCode, rightTypeCode);
                    //            break;
                    //        }
                    //    case BinaryOperation.Subtract:
                    //        {
                    //            // Check for defined
                    //            opReturnType = OpTable.GetSubtractOperationReturnType(leftTypeCode, rightTypeCode);
                    //            break;
                    //        }

                    //}

                    // Check for invalid
                    if(opReturnType == 0)
                    {
                        report.ReportDiagnostic(Code.NoBuiltInOperation, MessageSeverity.Error, Span, operationToken.Text, left.EvaluatedTypeSymbol, right.EvaluatedTypeSymbol);
                    }
                    else
                    {
                        // We can resolve the inferred return type
                        inferredTypeSymbol = provider.ResolveTypeSymbol(opReturnType, null, Span);
                    }
                }
                // Left and or right must be a custom type
                else
                {
                    // Check for primitive left
                    if(leftTypeCode != PrimitiveType.Any)
                    {
                        // We know that built in primitive types cannot support operation on user types
                        report.ReportDiagnostic(Code.NoBuiltInOperation, MessageSeverity.Error, Span, operationToken.Text, left.EvaluatedTypeSymbol, right.EvaluatedTypeSymbol);
                    }
                    else
                    {
                        // Must both be user types - check for operator defined on left
                    }
                }
            }
        }

        //public override ExpressionModel StaticallyEvaluateExpression(ISymbolProvider provider)
        //{
        //    // Check for resolved symbols
        //    if(left.EvaluatedTypeSymbol == null || right.EvaluatedTypeSymbol == null)
        //        return base.StaticallyEvaluateExpression(provider);

        //    // Check for both expressions can be evaluated statically
        //    bool bothExpressionsStaticallyEvaluated = left.IsStaticallyEvaluated == true && right.IsStaticallyEvaluated == true;

        //    // Evaluate left
        //    if(left.IsStaticallyEvaluated == true)
        //    {
        //        // Evaluate all static expressions
        //        left = left.StaticallyEvaluateExpression(provider);
        //    }

        //    // Evaluate right
        //    if(right.IsStaticallyEvaluated == true)
        //    {
        //        // Evaluate all static expressions
        //        right = right.StaticallyEvaluateExpression(provider);
        //    }

        //    // Check for both left and right statically evaluated
        //    if (bothExpressionsStaticallyEvaluated == true)
        //    {
        //        // Get left/right types
        //        PrimitiveType leftTypeCode = GetPromotedTypeCode(left.EvaluatedTypeSymbol);
        //        PrimitiveType rightTypeCode = GetPromotedTypeCode(right.EvaluatedTypeSymbol);

        //        // Evaluate left and right
        //        object leftVal = left.GetStaticallyEvaluatedValue();
        //        object rightVal = right.GetStaticallyEvaluatedValue();

        //        object evaluated = null;

        //        // Check operation type
        //        switch(operation)
        //        {
        //            case BinaryOperation.Equal:
        //                {
        //                    // Perform operation
        //                    evaluated = OpTable.GetEqualStaticallyEvaluatedValue(leftTypeCode, rightTypeCode, leftVal, rightVal);
        //                    break;
        //                }
        //            case BinaryOperation.NotEqual:
        //                {
        //                    evaluated = OpTable.GetNotEqualStaticallyEvaluatedValue(leftTypeCode, rightTypeCode, leftVal, rightVal);
        //                    break;
        //                }
        //            case BinaryOperation.Add:
        //                {
        //                    // Perform operation
        //                    evaluated = OpTable.GetAddStaticallyEvaluatedValue(leftTypeCode, rightTypeCode, leftVal, rightVal);
        //                    break;
        //                }
        //            case BinaryOperation.Subtract:
        //                {
        //                    // Perform operation
        //                    evaluated = OpTable.GetSubtractStaticallyEvaluatedValue(leftTypeCode, rightTypeCode, leftVal, rightVal);
        //                    break;
        //                }
        //            case BinaryOperation.Multiply:
        //                {
        //                    // Perform operation
        //                    evaluated = OpTable.GetMultiplyStaticallyEvaluatedValue(leftTypeCode, rightTypeCode, leftVal, rightVal);
        //                    break;
        //                }
        //            case BinaryOperation.Divide:
        //                {
        //                    // Perform operation
        //                    evaluated = OpTable.GetDivideStaticallyEvaluatedValue(leftTypeCode, rightTypeCode, leftVal, rightVal);
        //                    break;
        //                }
        //            case BinaryOperation.Greater:
        //                {
        //                    // Perform operation
        //                    evaluated = OpTable.GetGreaterStaticallyEvaluatedValue(leftTypeCode, rightTypeCode, leftVal, rightVal);
        //                    break;
        //                }
        //            case BinaryOperation.GreaterEqual:
        //                {
        //                    // Perform operation
        //                    evaluated = OpTable.GetGreaterEqualStaticallyEvaluatedValue(leftTypeCode, rightTypeCode, leftVal, rightVal);
        //                    break;
        //                }
        //            case BinaryOperation.Less:
        //                {
        //                    // Perform operation
        //                    evaluated = OpTable.GetLessStaticallyEvaluatedValue(leftTypeCode, rightTypeCode, leftVal, rightVal);
        //                    break;
        //                }
        //            case BinaryOperation.LessEqual:
        //                {
        //                    // Perform operation
        //                    evaluated = OpTable.GetLessEqualStaticallyEvaluatedValue(leftTypeCode, rightTypeCode, leftVal, rightVal);
        //                    break;
        //                }
        //        }

        //        // Check for optimized model provided
        //        if(evaluated != null)
        //        {
        //            // Create optimized model
        //            ConstantModel optimizedModel = new ConstantModel(Model, Parent, evaluated);

        //            // Resolve symbols
        //            optimizedModel.ResolveSymbols(provider, null);
        //            return optimizedModel;
        //        }
        //    }

        //    // Fallback to default behaviour
        //    return base.StaticallyEvaluateExpression(provider);
        //}

        private bool IsSmallPrimitive(ITypeReferenceSymbol type)
        {
            if(type.IsPrimitive == true)
            {
                switch(type.PrimitiveType)
                {
                    // Note - bool does not require promotion because it is represented as i32 by the runtime
                    case PrimitiveType.I8:
                    case PrimitiveType.U8:
                    case PrimitiveType.I16:
                    case PrimitiveType.U16:
                    case PrimitiveType.Char:
                        return true;
                }
            }
            return false;
        }

        private PrimitiveType GetPromotedTypeCode(ITypeReferenceSymbol type)
        {
            if(type.IsPrimitive == true)
            {
                switch(type.PrimitiveType)
                {
                    case PrimitiveType.I8:
                    case PrimitiveType.U8:
                    case PrimitiveType.I16:
                    case PrimitiveType.U16:
                    case PrimitiveType.Char:
                    case PrimitiveType.Bool:
                        return PrimitiveType.I32;
                }
            }
            return type.PrimitiveType;
        }

        private static BinaryOperation GetBinaryOperation(SyntaxToken op)
        {
            return op.Kind switch
            {
                SyntaxTokenKind.AddSymbol => BinaryOperation.Add,
                SyntaxTokenKind.SubtractSymbol => BinaryOperation.Subtract,
                SyntaxTokenKind.MultiplySymbol => BinaryOperation.Multiply,
                SyntaxTokenKind.DivideSymbol => BinaryOperation.Divide,
                SyntaxTokenKind.ModulusSymbol => BinaryOperation.Modulus,
                SyntaxTokenKind.GreaterSymbol => BinaryOperation.Greater,
                SyntaxTokenKind.GreaterEqualSymbol => BinaryOperation.GreaterEqual,
                SyntaxTokenKind.LessSymbol => BinaryOperation.Less,
                SyntaxTokenKind.LessEqualSymbol => BinaryOperation.LessEqual,
                SyntaxTokenKind.EqualitySymbol => BinaryOperation.Equal,
                SyntaxTokenKind.NonEqualitySymbol => BinaryOperation.NotEqual,
                SyntaxTokenKind.AndSymbol => BinaryOperation.And,
                SyntaxTokenKind.OrSymbol => BinaryOperation.Or,

                _ => throw new InvalidOperationException("Invalid binary operator: " + op.Kind)
            };
        }

        private static SyntaxToken CreateBinaryOperation(BinaryOperation op)
        {
            return Syntax.Token(op switch
            {
                BinaryOperation.Add => SyntaxTokenKind.AddSymbol,
                BinaryOperation.Subtract => SyntaxTokenKind.SubtractSymbol,
                BinaryOperation.Multiply => SyntaxTokenKind.MultiplySymbol,
                BinaryOperation.Divide => SyntaxTokenKind.DivideSymbol,
                BinaryOperation.Modulus => SyntaxTokenKind.ModulusSymbol,
                BinaryOperation.Greater => SyntaxTokenKind.GreaterSymbol,
                BinaryOperation.GreaterEqual => SyntaxTokenKind.GreaterEqualSymbol,
                BinaryOperation.Less => SyntaxTokenKind.LessSymbol,
                BinaryOperation.LessEqual => SyntaxTokenKind.LessEqualSymbol,
                BinaryOperation.Equal => SyntaxTokenKind.EqualitySymbol,
                BinaryOperation.NotEqual => SyntaxTokenKind.NonEqualitySymbol,
                BinaryOperation.And => SyntaxTokenKind.AndSymbol,
                BinaryOperation.Or => SyntaxTokenKind.OrSymbol,

                _ => throw new NotSupportedException(op.ToString())
            });
        }
    }
}
