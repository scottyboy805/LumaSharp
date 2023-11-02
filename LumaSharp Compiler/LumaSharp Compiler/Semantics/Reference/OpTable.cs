using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model;

namespace LumaSharp_Compiler.Semantics.Reference
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
        private delegate object Op(object a, object b);

        // Private
        private static Dictionary<(PrimitiveType, PrimitiveType), (PrimitiveType, Op)> addOpLookup = new Dictionary<(PrimitiveType, PrimitiveType), (PrimitiveType, Op)>
        {
            { (PrimitiveType.I32, PrimitiveType.I32), (PrimitiveType.I32, (a, b) => (int)a + (int)b) },
            { (PrimitiveType.I32, PrimitiveType.U32), (PrimitiveType.I64, (a, b) => (int)a + (uint)b) },
            { (PrimitiveType.I32, PrimitiveType.I64), (PrimitiveType.I64, (a, b) => (int)a + (long)b) },
            { (PrimitiveType.I32, PrimitiveType.U64), (PrimitiveType.U64, (a, b) => ((ulong)(int)a + (ulong)b)) },  // ??
            { (PrimitiveType.I32, PrimitiveType.Float), (PrimitiveType.Float, (a, b) => (int)a + (float)b) },
            { (PrimitiveType.I32, PrimitiveType.Double), (PrimitiveType.Double, (a, b) => (int)a + (double)b) },
        };
        private static Dictionary<(PrimitiveType, PrimitiveType), (PrimitiveType, Op)> subtractOpLookup = new Dictionary<(PrimitiveType, PrimitiveType), (PrimitiveType, Op)>
        {
            { (PrimitiveType.I32, PrimitiveType.I32), (PrimitiveType.I32, (a, b) => (int)a - (int)b) },
            { (PrimitiveType.I32, PrimitiveType.U32), (PrimitiveType.I64, (a, b) => (int)a - (uint)b) },
            { (PrimitiveType.I32, PrimitiveType.I64), (PrimitiveType.I64, (a, b) => (int)a - (long)b) },
            { (PrimitiveType.I32, PrimitiveType.U64), (PrimitiveType.U64, (a, b) => (ulong)(int)a - (ulong)b) },    // ??
            { (PrimitiveType.I32, PrimitiveType.Float), (PrimitiveType.Float, (a, b) => (int)a - (float)b) },
            { (PrimitiveType.I32, PrimitiveType.Double), (PrimitiveType.Double, (a, b) => (int)a - (double)b) },
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
        public static PrimitiveType GetAddOperationReturnType(PrimitiveType a, PrimitiveType b)
        {
            (PrimitiveType, Op) result;
            addOpLookup.TryGetValue((a, b), out result);
            return result.Item1;
        }

        public static object GetAddOperationStaticallyEvaluatedValue(PrimitiveType a, PrimitiveType b, object valA, object valB) 
        {
            (PrimitiveType, Op) result;
            addOpLookup.TryGetValue((a, b), out result);
            return result.Item2(valA, valB);
        }

        public static PrimitiveType GetSubtractOperationReturnType(PrimitiveType a, PrimitiveType b)
        {
            (PrimitiveType, Op) result;
            subtractOpLookup.TryGetValue((a, b), out result);
            return result.Item1;
        }

        public static object GetSubtractOperationStaticallyEvaluatedValue(PrimitiveType a, PrimitiveType b, object valA, object valB)
        {
            (PrimitiveType, Op) result;
            subtractOpLookup.TryGetValue((a, b), out result);
            return result.Item2(valA, valB);
        }

        public static void CheckSpecialOpUsage(MethodModel method, ISymbolProvider provider, ICompileReportProvider report)
        {
            // Get operator first
            string methodName = method.MethodName;

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
                    report.ReportMessage(Code.OperatorMustBeGlobal, MessageSeverity.Error, method.Syntax.StartToken.Source, methodName);
                }

                // Check usage
                switch (op)
                {
                    case SpecialOperator.OpEquals:
                        {
                            // Check return type
                            if(method.ReturnTypeSymbol.PrimitiveType != PrimitiveType.Bool)
                            {
                                report.ReportMessage(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Syntax.StartToken.Source, methodName, provider.ResolveTypeSymbol(PrimitiveType.Bool));
                            }

                            // Check for parameters
                            if (method.ParameterSymbols != null)
                            {
                                // Check parameter count
                                if (method.ParameterSymbols.Length != (method.IsGlobal == true ? 2 : 3))
                                {
                                    report.ReportMessage(Code.OperatorIncorrectParameterCount, MessageSeverity.Error, method.Syntax.StartToken.Source, methodName, 2);
                                }

                                // Check parameter types
                                for(int i = 0; i < 2 && i < method.ParameterSymbols.Length; i++)
                                {
                                    // Check for expected type
                                    if (method.ParameterSymbols[i].TypeSymbol != method.DeclaringTypeSymbol)
                                    {
                                        report.ReportMessage(Code.OperatorIncorrectParameter, MessageSeverity.Error, method.Syntax.StartToken.Source, methodName, method.DeclaringTypeSymbol);
                                    }
                                }
                            }
                            break;
                        }
                    case SpecialOperator.OpHash:
                        {
                            // Check return type
                            if (method.ReturnTypeSymbol.PrimitiveType != PrimitiveType.I32)
                            {
                                report.ReportMessage(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Syntax.StartToken.Source, methodName, provider.ResolveTypeSymbol(PrimitiveType.I32));
                            }

                            // Check for parameters
                            if (method.IsGlobal == true && method.ParameterSymbols != null && method.ParameterSymbols.Length > 0)
                            {
                                report.ReportMessage(Code.OperatorIncorrectVoidParameter, MessageSeverity.Error, method.Syntax.StartToken.Source, methodName);
                            }
                            break;
                        }
                    case SpecialOperator.OpString:
                        {
                            // Resolve string symbol
                            ITypeReferenceSymbol stringSymbol = provider.ResolveTypeSymbol(null, new TypeReferenceSyntax("string"));

                            // Check return type
                            if (method.ReturnTypeSymbol.PrimitiveType != PrimitiveType.Any && method.ReturnTypeSymbol != stringSymbol)
                            {
                                report.ReportMessage(Code.OperatorIncorrectReturn, MessageSeverity.Error, method.Syntax.StartToken.Source, methodName, stringSymbol);
                            }

                            // Check for parameters
                            if (method.IsGlobal == true && method.ParameterSymbols != null && method.ParameterSymbols.Length > 0)
                            {
                                report.ReportMessage(Code.OperatorIncorrectVoidParameter, MessageSeverity.Error, method.Syntax.StartToken.Source, methodName);
                            }
                            break;
                        }
                }
            }
        }
    }
}
