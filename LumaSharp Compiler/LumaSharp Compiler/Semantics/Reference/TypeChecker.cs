using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Reference
{
    internal static class TypeChecker
    {
        // Methods
        public static bool IsSpecialTypeAny(ITypeReferenceSymbol type)
        {
            if (type == null)
                return false;

            // Check library and name
            if(type.LibrarySymbol.LibraryName == "runtime" &&
                type.TypeName == "any")
            {
                return true;
            }
            return false;
        }

        public static bool IsSpecialTypeEnum(ITypeReferenceSymbol type)
        {
            if (type == null)
                return false;

            // Check library and name
            if(type.LibrarySymbol.LibraryName == "runtime" &&
                type.TypeName == "enum")
            {
                return true;
            }
            return false;
        }

        public static bool IsTypeAssignable(ITypeReferenceSymbol from, ITypeReferenceSymbol to)
        {
            // Check for trivial case
            if (from == to)
                return true;

            // Get primitive types
            PrimitiveType fromPrimitive = from.PrimitiveType;
            PrimitiveType toPrimitive = to.PrimitiveType;

            // Call types can be converted to special type `any` implicitly
            if (toPrimitive == PrimitiveType.Any && IsSpecialTypeAny(to) == true)
                return true;

            // Check for any
            if(fromPrimitive == PrimitiveType.Any || toPrimitive == PrimitiveType.Any)
            {
                // Check for assignable base type
                return HasBaseType(from, to);
            }
            else
            {
                // We can just compare primitive types
                return fromPrimitive == toPrimitive;
            }
        }

        public static bool HasBaseType(ITypeReferenceSymbol from, ITypeReferenceSymbol to)
        {
            if (from.BaseTypeSymbols != null)
            {
                // Check for base type
                foreach (ITypeReferenceSymbol baseType in from.BaseTypeSymbols)
                {
                    // Check for null
                    if (baseType == null)
                        continue;

                    // Check for base
                    if (baseType == to)
                        return true;

                    // Deep search
                    if (HasBaseType(baseType, to) == true)
                        return true;
                }
            }
            return false;
        }

        public static int GetBaseTypeDepth(ITypeReferenceSymbol type)
        {
            int depth = 0;
            GetBaseTypeDepth(type, ref depth);

            return depth;
        }

        public static int GetBaseTypeMatchDepth(ITypeReferenceSymbol from, ITypeReferenceSymbol to)
        {
            int depth = 0;
            GetBaseTypeMatchDepth(from, to, ref depth);

            return depth;
        }

        private static void GetBaseTypeDepth(ITypeReferenceSymbol type, ref int depth)
        {
            if(type != null && type.BaseTypeSymbols != null)
            {
                // Check for base type
                if(type.BaseTypeSymbols.Length > 0 && type.BaseTypeSymbols[0].IsType == true)
                {
                    depth++;
                    GetBaseTypeDepth(type.BaseTypeSymbols[0], ref depth);
                }
            }
        }

        private static void GetBaseTypeMatchDepth(ITypeReferenceSymbol from, ITypeReferenceSymbol to, ref int depth)
        {
            if (from != null && from.BaseTypeSymbols != null)
            {
                // Check for match
                if (from == to)
                    return;

                // Check for base type
                if (from.BaseTypeSymbols.Length > 0 && from.BaseTypeSymbols[0].IsType == true)
                {
                    depth++;
                    GetBaseTypeMatchDepth(from.BaseTypeSymbols[0], to, ref depth);
                }
            }
        }

        public static int GetTypeMatchScore(ITypeReferenceSymbol from, ITypeReferenceSymbol to)
        {
            // Make sure they are assignable first
            if (IsTypeAssignable(from, to) == false)
                return 0;

            // Get base type depth
            int depth = GetBaseTypeDepth(from);
            int score = depth - GetBaseTypeMatchDepth(from, to);

            // Get score
            return score;
        }
    }
}
