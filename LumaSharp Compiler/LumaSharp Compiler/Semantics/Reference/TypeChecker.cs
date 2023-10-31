using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Semantics.Reference
{
    internal static class TypeChecker
    {
        // Methods
        public static bool IsTypeAssignable(ITypeReferenceSymbol from, ITypeReferenceSymbol to)
        {
            // Check for trivial case
            if (from == to)
                return true;

            // Get primitive types
            PrimitiveType fromPrimitive = from.PrimitiveType;
            PrimitiveType toPrimitive = to.PrimitiveType;

            // Call types can be converted to special type `any` implicitly
            if (toPrimitive == PrimitiveType.Any)
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
            // Check for base type
            foreach(ITypeReferenceSymbol baseType in from.BaseTypeSymbols)
            {
                // Check for base
                if(baseType == to) 
                    return true;

                // Deep search
                if(HasBaseType(baseType, to) == true)
                    return true;
            }
            return false;
        }
    }
}
