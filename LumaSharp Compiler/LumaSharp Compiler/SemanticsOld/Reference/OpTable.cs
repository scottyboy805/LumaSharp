using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.SemanticsOld.Model;

namespace LumaSharp.Compiler.SemanticsOld.Reference
{
    internal static class OpTable
    {
        // Private
        private enum SpecialOperator
        {
            OpEquals,
            OpHash,
            OpString,
            OpAdd,
            OpSubtract,
            OpMultiply,
            OpDivide,
            OpNegate,
            OpGreater,
            OpGreaterEqual,
            OpLess,
            OpLessEqual,
        }

        // Delegate
        private delegate object UnaryOp(object val);
        private delegate object BinaryOp(object a, object b);

        // Private
        // Takes in left and right operand types are returns the return type (Also the type that the operands must be converted to in order to be evaluated)
        private readonly static Dictionary<(ReservedType, ReservedType), ReservedType> operationTable = new Dictionary<(ReservedType, ReservedType), ReservedType>
        {
            { (ReservedType.Int32, ReservedType.Int32), ReservedType.Int32 },
            { (ReservedType.Int32, ReservedType.UInt32), ReservedType.Int64 },
            { (ReservedType.Int32, ReservedType.Int64), ReservedType.Int64 },
            { (ReservedType.Int32, ReservedType.UInt64), ReservedType.UInt64 },
            { (ReservedType.Int32, ReservedType.Float), ReservedType.Float },
            { (ReservedType.Int32, ReservedType.Double), ReservedType.Double },
        };

        private readonly static Dictionary<ReservedType, BinaryOp> equalOperationTable = new Dictionary<ReservedType, BinaryOp>
        {
            { ReservedType.Int32, (a, b) => (int)a == (int)b },
            { ReservedType.Int64, (a, b) => (long)a == (long)b },
            { ReservedType.UInt64, (a, b) => (ulong)a == (ulong)b },
            { ReservedType.Float, (a, b) => (float)a == (float)b },
            { ReservedType.Double, (a, b) => (double)a == (double)b },
        };

        private readonly static Dictionary<ReservedType, BinaryOp> notEqualOperationTable = new Dictionary<ReservedType, BinaryOp>
        {
            { ReservedType.Int32, (a, b) => (int)a != (int)b },
            { ReservedType.Int64, (a, b) => (long)a != (long)b },
            { ReservedType.UInt64, (a, b) => (ulong)a != (ulong)b },
            { ReservedType.Float, (a, b) => (float)a != (float)b },
            { ReservedType.Double, (a, b) => (double)a != (double)b },
        };

        private readonly static Dictionary<ReservedType, BinaryOp> addOperationTable = new Dictionary<ReservedType, BinaryOp>
        {
            { ReservedType.Int32, (a, b) => (int)a + (int)b },
            { ReservedType.Int64, (a, b) => (long)a + (long)b },
            { ReservedType.UInt64, (a, b) => (ulong)a + (ulong)b },
            { ReservedType.Float, (a, b) => (float)a + (float)b },
            { ReservedType.Double, (a, b) => (double)a + (double)b },
        };

        private readonly static Dictionary<ReservedType, BinaryOp> subtractOperationTable = new Dictionary<ReservedType, BinaryOp>
        {
            { ReservedType.Int32, (a, b) => (int)a - (int)b },
            { ReservedType.Int64, (a, b) => (long)a - (long)b },
            { ReservedType.UInt64, (a, b) => (ulong)a - (ulong)b },
            { ReservedType.Float, (a, b) => (float)a - (float)b },
            { ReservedType.Double, (a, b) => (double)a - (double)b },
        };

        private readonly static Dictionary<ReservedType, BinaryOp> multiplyOperationTable = new Dictionary<ReservedType, BinaryOp>
        {
            { ReservedType.Int32, (a, b) => (int)a * (int)b },
            { ReservedType.Int64, (a, b) => (long) a * (long)b },
            { ReservedType.UInt64, (a, b) => (ulong)a * (ulong)b },
            { ReservedType.Float, (a, b) => (float)a * (float)b },
            { ReservedType.Double, (a, b) => (double)a * (double)b },
        };

        private readonly static Dictionary<ReservedType, BinaryOp> divideOperationTable = new Dictionary<ReservedType, BinaryOp>
        {
            { ReservedType.Int32, (a, b) => (int)a / (int)b },
            { ReservedType.Int64, (a, b) => (long)a / (long)b },
            { ReservedType.UInt64, (a, b) => (ulong)a / (ulong)b },
            { ReservedType.Float, (a, b) => (float)a / (float)b },
            { ReservedType.Double, (a, b) => (double)a / (double)b },
        };

        private readonly static Dictionary<ReservedType, UnaryOp> negateOperationTable = new Dictionary<ReservedType, UnaryOp>
        {
            { ReservedType.Int32, (val) => -(int)val },
            { ReservedType.Int64, (val) => -(long)val },
            //{ PrimitiveType.U64, (val) => -(ulong)val },
            { ReservedType.Float, (val) => -(float)val },
            { ReservedType.Double, (val) => -(double)val },
        };

        private readonly static Dictionary<ReservedType, BinaryOp> greaterOperationTable = new Dictionary<ReservedType, BinaryOp>
        {
            { ReservedType.Int32, (a, b) => (int)a > (int)b },
            { ReservedType.Int64, (a, b) => (long)a > (long)b },
            { ReservedType.UInt64, (a, b) => (ulong)a > (ulong)b },
            { ReservedType.Float, (a, b) => (float)a > (float)b },
            { ReservedType.Double, (a, b) => (double)a > (double)b },
        };

        private readonly static Dictionary<ReservedType, BinaryOp> greaterEqualOperationTable = new Dictionary<ReservedType, BinaryOp>
        {
            { ReservedType.Int32, (a, b) => (int)a >= (int)b },
            { ReservedType.Int64, (a, b) => (long)a >= (long)b },
            { ReservedType.UInt64, (a, b) => (ulong)a >= (ulong)b },
            { ReservedType.Float, (a, b) => (float)a >= (float)b },
            { ReservedType.Double, (a, b) => (double)a >= (double)b },
        };

        private readonly static Dictionary<ReservedType, BinaryOp> lessOperationTable = new Dictionary<ReservedType, BinaryOp>
        {
            { ReservedType.Int32, (a, b) => (int)a < (int)b },
            { ReservedType.Int64, (a, b) => (long)a < (long)b },
            { ReservedType.UInt64, (a, b) => (ulong)a < (ulong)b },
            { ReservedType.Float, (a, b) => (float)a < (float)b },
            { ReservedType.Double, (a, b) => (double)a < (double)b },
        };

        private readonly static Dictionary<ReservedType, BinaryOp> lessEqualOperationTable = new Dictionary<ReservedType, BinaryOp>
        {
            { ReservedType.Int32, (a, b) => (int)a < (int)b },
            { ReservedType.Int64, (a, b) => (long)a < (long)b },
            { ReservedType.UInt64, (a, b) => (ulong)a < (ulong)b },
            { ReservedType.Float, (a, b) => (float)a < (float)b },
            { ReservedType.Double, (a, b) => (double)a < (double)b },
        };


        public static readonly string[] specialOpMethods =
        {
            "op_equal",
            "op_hash",
            "op_string",
            "op_add",
            "op_subtract",
            "op_multiply",
            "op_divide",
            "op_negate",
            "op_greater",
            "op_greaterequal",
            "op_less",
            "op_lessequal",
        };

        // Methods
        public static ReservedType GetOperationReturnType(ReservedType left, ReservedType right)
        {
            ReservedType opType;
            operationTable.TryGetValue((left, right), out opType);
            return opType;
        }

        public static object GetEqualStaticallyEvaluatedValue(ReservedType left, ReservedType right, object leftVal, object rightVal)
        {
            ReservedType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select add op
            return equalOperationTable[opType](leftVal, rightVal);
        }

        public static object GetNotEqualStaticallyEvaluatedValue(ReservedType left, ReservedType right, object leftVal, object rightVal)
        {
            ReservedType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select add op
            return notEqualOperationTable[opType](leftVal, rightVal);
        }

        public static object GetAddStaticallyEvaluatedValue(ReservedType left, ReservedType right, object leftVal, object rightVal)
        {
            ReservedType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select add op
            return addOperationTable[opType](leftVal, rightVal);
        }

        public static object GetSubtractStaticallyEvaluatedValue(ReservedType left, ReservedType right, object leftVal, object rightVal)
        {
            ReservedType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return subtractOperationTable[opType](leftVal, rightVal);
        }

        public static object GetMultiplyStaticallyEvaluatedValue(ReservedType left, ReservedType right, object leftVal, object rightVal)
        {
            ReservedType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return multiplyOperationTable[opType](leftVal, rightVal);
        }

        public static object GetDivideStaticallyEvaluatedValue(ReservedType left, ReservedType right, object leftVal, object rightVal)
        {
            ReservedType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return divideOperationTable[opType](leftVal, rightVal);
        }

        public static object GetGreaterStaticallyEvaluatedValue(ReservedType left, ReservedType right, object leftVal, object rightVal)
        {
            ReservedType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return greaterOperationTable[opType](leftVal, rightVal);
        }

        public static object GetGreaterEqualStaticallyEvaluatedValue(ReservedType left, ReservedType right, object leftVal, object rightVal)
        {
            ReservedType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return greaterEqualOperationTable[opType](leftVal, rightVal);
        }

        public static object GetLessStaticallyEvaluatedValue(ReservedType left, ReservedType right, object leftVal, object rightVal)
        {
            ReservedType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return lessOperationTable[opType](leftVal, rightVal);
        }

        public static object GetLessEqualStaticallyEvaluatedValue(ReservedType left, ReservedType right, object leftVal, object rightVal)
        {
            ReservedType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return lessEqualOperationTable[opType](leftVal, rightVal);
        }

        public static void CheckSpecialOpUsage(MethodModel method, ISymbolProvider provider, ICompileReportProvider report)
        {
            // Get operator first
            string methodName = method.IdentifierName;

            // Get return type - special operators should only have 1
            ITypeReferenceSymbol returnTypeSymbol = method.ReturnTypeSymbols[0];

            // Try to get index
            int index = Array.IndexOf(specialOpMethods, methodName);

            // Check for found
            if (index != -1)
            {
                // Get method type
                SpecialOperator op = (SpecialOperator)index;

                // Check for global
                if(method.IsStatic == false)
                {
                    report.ReportDiagnostic(Code.OperatorMustBeGlobal, MessageSeverity.Error, method.Span, methodName);
                }

                // Check usage
                switch (op)
                {
                    case SpecialOperator.OpEquals:
                        {
                            // Check return type
                            if(returnTypeSymbol.PrimitiveType != ReservedType.Bool)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, provider.ResolveTypeSymbol(ReservedType.Bool, null, method.Span));
                            }

                            // Check for parameters
                            if (method.ParameterSymbols != null)
                            {
                                // Check parameter count
                                if (method.ParameterSymbols.Length != (method.IsStatic == true ? 2 : 3))
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameterCount, MessageSeverity.Error, method.Span, methodName, 2);
                                }

                                // Check parameter types
                                for(int i = 0; i < 2 && i < method.ParameterSymbols.Length; i++)
                                {
                                    // Check for expected type
                                    if (method.ParameterSymbols[i].TypeSymbol != method.DeclaringTypeSymbol)
                                    {
                                        report.ReportDiagnostic(Code.OperatorIncorrectParameter, MessageSeverity.Error, method.Span, methodName, method.DeclaringTypeSymbol);
                                    }
                                }
                            }
                            break;
                        }
                    case SpecialOperator.OpHash:
                        {
                            // Check return type
                            if (returnTypeSymbol.PrimitiveType != ReservedType.Int32)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, provider.ResolveTypeSymbol(ReservedType.Int32, null, method.Span));
                            }

                            // Check for parameters
                            if (method.IsStatic == true && method.ParameterSymbols != null && method.ParameterSymbols.Length > 0)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectVoidParameter, MessageSeverity.Error, method.Span, methodName);
                            }
                            break;
                        }
                    case SpecialOperator.OpString:
                        {
                            // Resolve string symbol
                            ITypeReferenceSymbol stringSymbol = provider.ResolveTypeSymbol(ReservedType.String, null, method.Span);

                            // Check return type
                            if (returnTypeSymbol.PrimitiveType != ReservedType.Object && returnTypeSymbol != stringSymbol)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, stringSymbol);
                            }

                            // Check for parameters
                            if (method.IsStatic == true && method.ParameterSymbols != null && method.ParameterSymbols.Length > 0)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectVoidParameter, MessageSeverity.Error, method.Span, methodName);
                            }
                            break;
                        }
                    case SpecialOperator.OpAdd:
                        {
                            // Check parameter count
                            if (method.IsStatic == true)
                            {
                                if (method.ParameterSymbols == null || method.ParameterSymbols.Length != 2)
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameterCount, MessageSeverity.Error, method.Span, methodName, 2);
                                }

                                // First parameter must be of declaring type
                                if(method.ParameterSymbols != null && method.ParameterSymbols.Length >= 1 && method.ParameterSymbols[0].TypeSymbol != method.DeclaringTypeSymbol)
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameter, MessageSeverity.Error, method.Span, methodName, method.DeclaringTypeSymbol);
                                }
                            }
                            break;
                        }
                    case SpecialOperator.OpSubtract:
                        {
                            // Check parameter count
                            if (method.IsStatic == true)
                            {
                                if (method.ParameterSymbols == null || method.ParameterSymbols.Length != 2)
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameterCount, MessageSeverity.Error, method.Span, methodName, 2);
                                }

                                // First parameter must be of declaring type
                                if (method.ParameterSymbols != null && method.ParameterSymbols.Length >= 1 && method.ParameterSymbols[0].TypeSymbol != method.DeclaringTypeSymbol)
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameter, MessageSeverity.Error, method.Span, methodName, method.DeclaringTypeSymbol);
                                }
                            }
                            break;
                        }
                    case SpecialOperator.OpMultiply:
                        {
                            // Check parameter count
                            if (method.IsStatic == true)
                            {
                                if (method.ParameterSymbols == null || method.ParameterSymbols.Length != 2)
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameterCount, MessageSeverity.Error, method.Span, methodName, 2);
                                }

                                // First parameter must be of declaring type
                                if (method.ParameterSymbols != null && method.ParameterSymbols.Length >= 1 && method.ParameterSymbols[0].TypeSymbol != method.DeclaringTypeSymbol)
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameter, MessageSeverity.Error, method.Span, methodName, method.DeclaringTypeSymbol);
                                }
                            }
                            break;
                        }
                    case SpecialOperator.OpDivide:
                        {
                            // Check parameter count
                            if (method.IsStatic == true)
                            {
                                if (method.ParameterSymbols == null || method.ParameterSymbols.Length != 2)
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameterCount, MessageSeverity.Error, method.Span, methodName, 2);
                                }

                                // First parameter must be of declaring type
                                if (method.ParameterSymbols != null && method.ParameterSymbols.Length >= 1 && method.ParameterSymbols[0].TypeSymbol != method.DeclaringTypeSymbol)
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameter, MessageSeverity.Error, method.Span, methodName, method.DeclaringTypeSymbol);
                                }
                            }
                            break;
                        }
                    case SpecialOperator.OpNegate:
                        {
                            // Check return type
                            if(returnTypeSymbol != method.DeclaringTypeSymbol)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, method.DeclaringTypeSymbol);
                            }

                            // Check parameter count
                            if(method.IsStatic == true)
                            {
                                // Check for incorrect parameter count
                                if(method.ParameterSymbols == null || method.ParameterSymbols.Length != 1)
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameterCount, MessageSeverity.Error, method.Span, methodName, 1);
                                }

                                // Single parameter must be of declaring type
                                if(method.ParameterSymbols != null && method.ParameterSymbols.Length == 1 && method.ParameterSymbols[0].TypeSymbol != method.DeclaringTypeSymbol)
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameter, MessageSeverity.Error, method.Span, methodName, method.DeclaringTypeSymbol);
                                }
                            }
                            break;
                        }
                    case SpecialOperator.OpGreater:
                        {
                            // Check return type
                            if (returnTypeSymbol.PrimitiveType != ReservedType.Bool)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, provider.ResolveTypeSymbol(ReservedType.Bool, null, method.Span));
                            }

                            // Check for parameters
                            if (method.ParameterSymbols != null)
                            {
                                // Check parameter count
                                if (method.ParameterSymbols.Length != (method.IsStatic == true ? 2 : 3))
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameterCount, MessageSeverity.Error, method.Span, methodName, 2);
                                }

                                // Check parameter types
                                if(method.ParameterSymbols.Length > 0)
                                {
                                    // Check for expected type
                                    if (method.ParameterSymbols[0].TypeSymbol != method.DeclaringTypeSymbol)
                                    {
                                        report.ReportDiagnostic(Code.OperatorIncorrectParameter, MessageSeverity.Error, method.Span, methodName, method.DeclaringTypeSymbol);
                                    }
                                }
                            }
                            break;
                        }
                    case SpecialOperator.OpGreaterEqual:
                        {
                            // Check return type
                            if (returnTypeSymbol.PrimitiveType != ReservedType.Bool)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, provider.ResolveTypeSymbol(ReservedType.Bool, null, method.Span));
                            }

                            // Check for parameters
                            if (method.ParameterSymbols != null)
                            {
                                // Check parameter count
                                if (method.ParameterSymbols.Length != (method.IsStatic == true ? 2 : 3))
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameterCount, MessageSeverity.Error, method.Span, methodName, 2);
                                }

                                // Check parameter types
                                if (method.ParameterSymbols.Length > 0)
                                {
                                    // Check for expected type
                                    if (method.ParameterSymbols[0].TypeSymbol != method.DeclaringTypeSymbol)
                                    {
                                        report.ReportDiagnostic(Code.OperatorIncorrectParameter, MessageSeverity.Error, method.Span, methodName, method.DeclaringTypeSymbol);
                                    }
                                }
                            }
                            break;
                        }
                    case SpecialOperator.OpLess:
                        {
                            // Check return type
                            if (returnTypeSymbol.PrimitiveType != ReservedType.Bool)
                            {
                                //report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, provider.ResolveTypeSymbol(PrimitiveType.Bool, null, method.MethodSyntax.StartToken.Span));
                            }

                            // Check for parameters
                            if (method.ParameterSymbols != null)
                            {
                                // Check parameter count
                                if (method.ParameterSymbols.Length != (method.IsStatic == true ? 2 : 3))
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameterCount, MessageSeverity.Error, method.Span, methodName, 2);
                                }

                                // Check parameter types
                                if (method.ParameterSymbols.Length > 0)
                                {
                                    // Check for expected type
                                    if (method.ParameterSymbols[0].TypeSymbol != method.DeclaringTypeSymbol)
                                    {
                                        report.ReportDiagnostic(Code.OperatorIncorrectParameter, MessageSeverity.Error, method.Span, methodName, method.DeclaringTypeSymbol);
                                    }
                                }
                            }
                            break;
                        }
                    case SpecialOperator.OpLessEqual:
                        {
                            // Check return type
                            if (returnTypeSymbol.PrimitiveType != ReservedType.Bool)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, provider.ResolveTypeSymbol(ReservedType.Bool, null, method.Span));
                            }

                            // Check for parameters
                            if (method.ParameterSymbols != null)
                            {
                                // Check parameter count
                                if (method.ParameterSymbols.Length != (method.IsStatic == true ? 2 : 3))
                                {
                                    report.ReportDiagnostic(Code.OperatorIncorrectParameterCount, MessageSeverity.Error, method.Span, methodName, 2);
                                }

                                // Check parameter types
                                if (method.ParameterSymbols.Length > 0)
                                {
                                    // Check for expected type
                                    if (method.ParameterSymbols[0].TypeSymbol != method.DeclaringTypeSymbol)
                                    {
                                        report.ReportDiagnostic(Code.OperatorIncorrectParameter, MessageSeverity.Error, method.Span, methodName, method.DeclaringTypeSymbol);
                                    }
                                }
                            }
                            break;
                        }
                }
            }
        }
    }
}
