using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Model;

namespace LumaSharp.Compiler.Semantics.Reference
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
        private readonly static Dictionary<(PrimitiveType, PrimitiveType), PrimitiveType> operationTable = new Dictionary<(PrimitiveType, PrimitiveType), PrimitiveType>
        {
            { (PrimitiveType.I32, PrimitiveType.I32), PrimitiveType.I32 },
            { (PrimitiveType.I32, PrimitiveType.U32), PrimitiveType.I64 },
            { (PrimitiveType.I32, PrimitiveType.I64), PrimitiveType.I64 },
            { (PrimitiveType.I32, PrimitiveType.U64), PrimitiveType.U64 },
            { (PrimitiveType.I32, PrimitiveType.F32), PrimitiveType.F32 },
            { (PrimitiveType.I32, PrimitiveType.F64), PrimitiveType.F64 },
        };

        private readonly static Dictionary<PrimitiveType, BinaryOp> equalOperationTable = new Dictionary<PrimitiveType, BinaryOp>
        {
            { PrimitiveType.I32, (a, b) => (int)a == (int)b },
            { PrimitiveType.I64, (a, b) => (long)a == (long)b },
            { PrimitiveType.U64, (a, b) => (ulong)a == (ulong)b },
            { PrimitiveType.F32, (a, b) => (float)a == (float)b },
            { PrimitiveType.F64, (a, b) => (double)a == (double)b },
        };

        private readonly static Dictionary<PrimitiveType, BinaryOp> notEqualOperationTable = new Dictionary<PrimitiveType, BinaryOp>
        {
            { PrimitiveType.I32, (a, b) => (int)a != (int)b },
            { PrimitiveType.I64, (a, b) => (long)a != (long)b },
            { PrimitiveType.U64, (a, b) => (ulong)a != (ulong)b },
            { PrimitiveType.F32, (a, b) => (float)a != (float)b },
            { PrimitiveType.F64, (a, b) => (double)a != (double)b },
        };

        private readonly static Dictionary<PrimitiveType, BinaryOp> addOperationTable = new Dictionary<PrimitiveType, BinaryOp>
        {
            { PrimitiveType.I32, (a, b) => (int)a + (int)b },
            { PrimitiveType.I64, (a, b) => (long)a + (long)b },
            { PrimitiveType.U64, (a, b) => (ulong)a + (ulong)b },
            { PrimitiveType.F32, (a, b) => (float)a + (float)b },
            { PrimitiveType.F64, (a, b) => (double)a + (double)b },
        };

        private readonly static Dictionary<PrimitiveType, BinaryOp> subtractOperationTable = new Dictionary<PrimitiveType, BinaryOp>
        {
            { PrimitiveType.I32, (a, b) => (int)a - (int)b },
            { PrimitiveType.I64, (a, b) => (long)a - (long)b },
            { PrimitiveType.U64, (a, b) => (ulong)a - (ulong)b },
            { PrimitiveType.F32, (a, b) => (float)a - (float)b },
            { PrimitiveType.F64, (a, b) => (double)a - (double)b },
        };

        private readonly static Dictionary<PrimitiveType, BinaryOp> multiplyOperationTable = new Dictionary<PrimitiveType, BinaryOp>
        {
            { PrimitiveType.I32, (a, b) => (int)a * (int)b },
            { PrimitiveType.I64, (a, b) => (long) a * (long)b },
            { PrimitiveType.U64, (a, b) => (ulong)a * (ulong)b },
            { PrimitiveType.F32, (a, b) => (float)a * (float)b },
            { PrimitiveType.F64, (a, b) => (double)a * (double)b },
        };

        private readonly static Dictionary<PrimitiveType, BinaryOp> divideOperationTable = new Dictionary<PrimitiveType, BinaryOp>
        {
            { PrimitiveType.I32, (a, b) => (int)a / (int)b },
            { PrimitiveType.I64, (a, b) => (long)a / (long)b },
            { PrimitiveType.U64, (a, b) => (ulong)a / (ulong)b },
            { PrimitiveType.F32, (a, b) => (float)a / (float)b },
            { PrimitiveType.F64, (a, b) => (double)a / (double)b },
        };

        private readonly static Dictionary<PrimitiveType, UnaryOp> negateOperationTable = new Dictionary<PrimitiveType, UnaryOp>
        {
            { PrimitiveType.I32, (val) => -(int)val },
            { PrimitiveType.I64, (val) => -(long)val },
            //{ PrimitiveType.U64, (val) => -(ulong)val },
            { PrimitiveType.F32, (val) => -(float)val },
            { PrimitiveType.F64, (val) => -(double)val },
        };

        private readonly static Dictionary<PrimitiveType, BinaryOp> greaterOperationTable = new Dictionary<PrimitiveType, BinaryOp>
        {
            { PrimitiveType.I32, (a, b) => (int)a > (int)b },
            { PrimitiveType.I64, (a, b) => (long)a > (long)b },
            { PrimitiveType.U64, (a, b) => (ulong)a > (ulong)b },
            { PrimitiveType.F32, (a, b) => (float)a > (float)b },
            { PrimitiveType.F64, (a, b) => (double)a > (double)b },
        };

        private readonly static Dictionary<PrimitiveType, BinaryOp> greaterEqualOperationTable = new Dictionary<PrimitiveType, BinaryOp>
        {
            { PrimitiveType.I32, (a, b) => (int)a >= (int)b },
            { PrimitiveType.I64, (a, b) => (long)a >= (long)b },
            { PrimitiveType.U64, (a, b) => (ulong)a >= (ulong)b },
            { PrimitiveType.F32, (a, b) => (float)a >= (float)b },
            { PrimitiveType.F64, (a, b) => (double)a >= (double)b },
        };

        private readonly static Dictionary<PrimitiveType, BinaryOp> lessOperationTable = new Dictionary<PrimitiveType, BinaryOp>
        {
            { PrimitiveType.I32, (a, b) => (int)a < (int)b },
            { PrimitiveType.I64, (a, b) => (long)a < (long)b },
            { PrimitiveType.U64, (a, b) => (ulong)a < (ulong)b },
            { PrimitiveType.F32, (a, b) => (float)a < (float)b },
            { PrimitiveType.F64, (a, b) => (double)a < (double)b },
        };

        private readonly static Dictionary<PrimitiveType, BinaryOp> lessEqualOperationTable = new Dictionary<PrimitiveType, BinaryOp>
        {
            { PrimitiveType.I32, (a, b) => (int)a < (int)b },
            { PrimitiveType.I64, (a, b) => (long)a < (long)b },
            { PrimitiveType.U64, (a, b) => (ulong)a < (ulong)b },
            { PrimitiveType.F32, (a, b) => (float)a < (float)b },
            { PrimitiveType.F64, (a, b) => (double)a < (double)b },
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
        public static PrimitiveType GetOperationReturnType(PrimitiveType left, PrimitiveType right)
        {
            PrimitiveType opType;
            operationTable.TryGetValue((left, right), out opType);
            return opType;
        }

        public static object GetEqualStaticallyEvaluatedValue(PrimitiveType left, PrimitiveType right, object leftVal, object rightVal)
        {
            PrimitiveType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select add op
            return equalOperationTable[opType](leftVal, rightVal);
        }

        public static object GetNotEqualStaticallyEvaluatedValue(PrimitiveType left, PrimitiveType right, object leftVal, object rightVal)
        {
            PrimitiveType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select add op
            return notEqualOperationTable[opType](leftVal, rightVal);
        }

        public static object GetAddStaticallyEvaluatedValue(PrimitiveType left, PrimitiveType right, object leftVal, object rightVal)
        {
            PrimitiveType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select add op
            return addOperationTable[opType](leftVal, rightVal);
        }

        public static object GetSubtractStaticallyEvaluatedValue(PrimitiveType left, PrimitiveType right, object leftVal, object rightVal)
        {
            PrimitiveType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return subtractOperationTable[opType](leftVal, rightVal);
        }

        public static object GetMultiplyStaticallyEvaluatedValue(PrimitiveType left, PrimitiveType right, object leftVal, object rightVal)
        {
            PrimitiveType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return multiplyOperationTable[opType](leftVal, rightVal);
        }

        public static object GetDivideStaticallyEvaluatedValue(PrimitiveType left, PrimitiveType right, object leftVal, object rightVal)
        {
            PrimitiveType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return divideOperationTable[opType](leftVal, rightVal);
        }

        public static object GetGreaterStaticallyEvaluatedValue(PrimitiveType left, PrimitiveType right, object leftVal, object rightVal)
        {
            PrimitiveType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return greaterOperationTable[opType](leftVal, rightVal);
        }

        public static object GetGreaterEqualStaticallyEvaluatedValue(PrimitiveType left, PrimitiveType right, object leftVal, object rightVal)
        {
            PrimitiveType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return greaterEqualOperationTable[opType](leftVal, rightVal);
        }

        public static object GetLessStaticallyEvaluatedValue(PrimitiveType left, PrimitiveType right, object leftVal, object rightVal)
        {
            PrimitiveType opType;
            operationTable.TryGetValue((left, right), out opType);

            // Select subtract op
            return lessOperationTable[opType](leftVal, rightVal);
        }

        public static object GetLessEqualStaticallyEvaluatedValue(PrimitiveType left, PrimitiveType right, object leftVal, object rightVal)
        {
            PrimitiveType opType;
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
                if(method.IsGlobal == false)
                {
                    report.ReportDiagnostic(Code.OperatorMustBeGlobal, MessageSeverity.Error, method.Span, methodName);
                }

                // Check usage
                switch (op)
                {
                    case SpecialOperator.OpEquals:
                        {
                            // Check return type
                            if(returnTypeSymbol.PrimitiveType != PrimitiveType.Bool)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, provider.ResolveTypeSymbol(PrimitiveType.Bool, null, method.Span));
                            }

                            // Check for parameters
                            if (method.ParameterSymbols != null)
                            {
                                // Check parameter count
                                if (method.ParameterSymbols.Length != (method.IsGlobal == true ? 2 : 3))
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
                            if (returnTypeSymbol.PrimitiveType != PrimitiveType.I32)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, provider.ResolveTypeSymbol(PrimitiveType.I32, null, method.Span));
                            }

                            // Check for parameters
                            if (method.IsGlobal == true && method.ParameterSymbols != null && method.ParameterSymbols.Length > 0)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectVoidParameter, MessageSeverity.Error, method.Span, methodName);
                            }
                            break;
                        }
                    case SpecialOperator.OpString:
                        {
                            // Resolve string symbol
                            ITypeReferenceSymbol stringSymbol = provider.ResolveTypeSymbol(PrimitiveType.String, null, method.Span);

                            // Check return type
                            if (returnTypeSymbol.PrimitiveType != PrimitiveType.Any && returnTypeSymbol != stringSymbol)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, stringSymbol);
                            }

                            // Check for parameters
                            if (method.IsGlobal == true && method.ParameterSymbols != null && method.ParameterSymbols.Length > 0)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectVoidParameter, MessageSeverity.Error, method.Span, methodName);
                            }
                            break;
                        }
                    case SpecialOperator.OpAdd:
                        {
                            // Check parameter count
                            if (method.IsGlobal == true)
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
                            if (method.IsGlobal == true)
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
                            if (method.IsGlobal == true)
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
                            if (method.IsGlobal == true)
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
                            if(method.IsGlobal == true)
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
                            if (returnTypeSymbol.PrimitiveType != PrimitiveType.Bool)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, provider.ResolveTypeSymbol(PrimitiveType.Bool, null, method.Span));
                            }

                            // Check for parameters
                            if (method.ParameterSymbols != null)
                            {
                                // Check parameter count
                                if (method.ParameterSymbols.Length != (method.IsGlobal == true ? 2 : 3))
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
                            if (returnTypeSymbol.PrimitiveType != PrimitiveType.Bool)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, provider.ResolveTypeSymbol(PrimitiveType.Bool, null, method.Span));
                            }

                            // Check for parameters
                            if (method.ParameterSymbols != null)
                            {
                                // Check parameter count
                                if (method.ParameterSymbols.Length != (method.IsGlobal == true ? 2 : 3))
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
                            if (returnTypeSymbol.PrimitiveType != PrimitiveType.Bool)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, provider.ResolveTypeSymbol(PrimitiveType.Bool, null, method.MethodSyntax.StartToken.Span));
                            }

                            // Check for parameters
                            if (method.ParameterSymbols != null)
                            {
                                // Check parameter count
                                if (method.ParameterSymbols.Length != (method.IsGlobal == true ? 2 : 3))
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
                            if (returnTypeSymbol.PrimitiveType != PrimitiveType.Bool)
                            {
                                report.ReportDiagnostic(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Span, methodName, provider.ResolveTypeSymbol(PrimitiveType.Bool, null, method.Span));
                            }

                            // Check for parameters
                            if (method.ParameterSymbols != null)
                            {
                                // Check parameter count
                                if (method.ParameterSymbols.Length != (method.IsGlobal == true ? 2 : 3))
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
