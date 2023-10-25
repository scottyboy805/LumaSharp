using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Semantics.Reference
{
    internal static class OpTable
    {
        // Private
        private static PrimitiveType[,,] addOpLookup =
        {
            {
                { PrimitiveType.I8, PrimitiveType.I8, PrimitiveType.I32, },
                { PrimitiveType.I8, PrimitiveType.I16, PrimitiveType.I32, },
                { PrimitiveType.I8, PrimitiveType.I32, PrimitiveType.I32, },
                { PrimitiveType.I8, PrimitiveType.I64, PrimitiveType.I64, },
                { PrimitiveType.I8, PrimitiveType.Float, PrimitiveType.Float, },
                { PrimitiveType.I8, PrimitiveType.Double, PrimitiveType.Double, },

                { PrimitiveType.I16, PrimitiveType.I8, PrimitiveType.I32, },
                { PrimitiveType.I16, PrimitiveType.I16, PrimitiveType.I32, },
                { PrimitiveType.I16, PrimitiveType.I32, PrimitiveType.I32, },
                { PrimitiveType.I16, PrimitiveType.I64, PrimitiveType.I64, },
                { PrimitiveType.I16, PrimitiveType.Float, PrimitiveType.Float, },
                { PrimitiveType.I16, PrimitiveType.Double, PrimitiveType.Double, },

                { PrimitiveType.I32, PrimitiveType.I8, PrimitiveType.I32, },
                { PrimitiveType.I32, PrimitiveType.I16, PrimitiveType.I32, },
                { PrimitiveType.I32, PrimitiveType.I32, PrimitiveType.I32, },
                { PrimitiveType.I32, PrimitiveType.I64, PrimitiveType.I64, },
                { PrimitiveType.I32, PrimitiveType.Float, PrimitiveType.Float, },
                { PrimitiveType.I32, PrimitiveType.Double, PrimitiveType.Double, },

                { PrimitiveType.I64, PrimitiveType.I8, PrimitiveType.I64, },
                { PrimitiveType.I64, PrimitiveType.I16, PrimitiveType.I64, },
                { PrimitiveType.I64, PrimitiveType.I32, PrimitiveType.I64, },
                { PrimitiveType.I64, PrimitiveType.I64, PrimitiveType.I64, },
                { PrimitiveType.I64, PrimitiveType.Float, PrimitiveType.Double, },
                { PrimitiveType.I64, PrimitiveType.Double, PrimitiveType.Double, },


                { PrimitiveType.Float, PrimitiveType.Float, PrimitiveType.Float, },
                { PrimitiveType.Float, PrimitiveType.I32, PrimitiveType.Float, },
                { PrimitiveType.I32, PrimitiveType.Float, PrimitiveType.Float, },
                { PrimitiveType.Double, PrimitiveType.Double, PrimitiveType.Double, },
                { PrimitiveType.Double, PrimitiveType.Float, PrimitiveType.Double, },
                { PrimitiveType.Float, PrimitiveType.Double, PrimitiveType.Double, },
            },
        };
    }
}
