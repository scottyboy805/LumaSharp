using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Semantics.Reference
{
    internal static class OpTable
    {
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
    }
}
