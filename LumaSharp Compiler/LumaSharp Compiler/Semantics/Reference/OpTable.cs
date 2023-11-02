using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Semantics.Reference
{
    internal static class OpTable
    {
        // Private
        private static Dictionary<(PrimitiveType, PrimitiveType), PrimitiveType> addOpLookup = new Dictionary<(PrimitiveType, PrimitiveType), PrimitiveType>
        {
            { (PrimitiveType.I32, PrimitiveType.I32), PrimitiveType.I32 },
            { (PrimitiveType.I32, PrimitiveType.U32), PrimitiveType.I64 },
            { (PrimitiveType.I32, PrimitiveType.I64), PrimitiveType.I64 },
            { (PrimitiveType.I32, PrimitiveType.U64), PrimitiveType.U64 },
            { (PrimitiveType.I32, PrimitiveType.Float), PrimitiveType.Float },
            { (PrimitiveType.I32, PrimitiveType.Double), PrimitiveType.Double },
        };
        //private static PrimitiveType[,] addOpLookup =
        //{
        //    //{
        //    //    { PrimitiveType.I32, PrimitiveType.I32, PrimitiveType.I32, },
        //    //    { PrimitiveType.I32, PrimitiveType.U32, PrimitiveType.U32 },
        //    //    { PrimitiveType.I32, PrimitiveType.I64, PrimitiveType.I64, },
        //    //    { PrimitiveType.I32, PrimitiveType.Float, PrimitiveType.Float, },
        //    //    { PrimitiveType.I32, PrimitiveType.Double, PrimitiveType.Double, },

        //    //    { PrimitiveType.I64, PrimitiveType.I32, PrimitiveType.I64, },
        //    //    { PrimitiveType.I64, PrimitiveType.I64, PrimitiveType.I64, },
        //    //    { PrimitiveType.I64, PrimitiveType.Float, PrimitiveType.Double, },
        //    //    { PrimitiveType.I64, PrimitiveType.Double, PrimitiveType.Double, },

        //    //    { PrimitiveType.Float, PrimitiveType.Float, PrimitiveType.Float, },
        //    //    { PrimitiveType.Float, PrimitiveType.I32, PrimitiveType.Float, },
        //    //    { PrimitiveType.I32, PrimitiveType.Float, PrimitiveType.Float, },
        //    //    { PrimitiveType.Double, PrimitiveType.Double, PrimitiveType.Double, },
        //    //    { PrimitiveType.Double, PrimitiveType.Float, PrimitiveType.Double, },
        //    //    { PrimitiveType.Float, PrimitiveType.Double, PrimitiveType.Double, },
        //    //},
        //};

        public static PrimitiveType GetAddOperationReturnType(PrimitiveType a, PrimitiveType b)
        {
            PrimitiveType result;
            addOpLookup.TryGetValue((a, b), out result);
            return result;
        }
    }
}
