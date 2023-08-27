using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics.Reference
{
    internal static class Types
    {
        // Type
        internal sealed class BuiltIn_Primitive : ILibraryReferenceSymbol, ITypeReferenceSymbol
        {
            // Private
            private PrimitiveType primitiveType = 0;

            // Properties
            public string TypeName => primitiveType.ToString().ToLower();
            public int SymbolToken => (int)primitiveType;

            public PrimitiveType PrimitiveType => primitiveType;
            public bool IsPrimitive => true;
            public bool IsType => false;
            public bool IsContract => false;
            public bool IsEnum => false;

            public ITypeReferenceSymbol[] GenericTypeSymbols => null;
            public ITypeReferenceSymbol[] BaseTypeSymbols => null;
            public ILibraryReferenceSymbol LibrarySymbol => null;

            public string LibraryName => "runtime";

            public IFieldReferenceSymbol[] FieldMemberSymbols => throw new NotImplementedException();

            public IFieldReferenceSymbol[] AccessorMemberSymbols => throw new NotImplementedException();

            public IMethodReferenceSymbol[] MethodMemberSymbols => throw new NotImplementedException();


            // Constructor
            internal BuiltIn_Primitive(PrimitiveType primitiveType)
            {
                this.primitiveType = primitiveType;
            }
        }

        // Public
        public static readonly BuiltIn_Primitive any = new BuiltIn_Primitive(PrimitiveType.Any);
        public static readonly BuiltIn_Primitive _bool = new BuiltIn_Primitive(PrimitiveType.Bool);
        public static readonly BuiltIn_Primitive _char = new BuiltIn_Primitive(PrimitiveType.Char);
        public static readonly BuiltIn_Primitive _string = new BuiltIn_Primitive(PrimitiveType.String);

        public static readonly BuiltIn_Primitive i8 = new BuiltIn_Primitive(PrimitiveType.I8);
        public static readonly BuiltIn_Primitive u8 = new BuiltIn_Primitive(PrimitiveType.U8);
        public static readonly BuiltIn_Primitive i16 = new BuiltIn_Primitive(PrimitiveType.I16);
        public static readonly BuiltIn_Primitive u16 = new BuiltIn_Primitive(PrimitiveType.U16);
        public static readonly BuiltIn_Primitive i32 = new BuiltIn_Primitive(PrimitiveType.I32);
        public static readonly BuiltIn_Primitive u32 = new BuiltIn_Primitive(PrimitiveType.U32);
        public static readonly BuiltIn_Primitive i64 = new BuiltIn_Primitive(PrimitiveType.I64);
        public static readonly BuiltIn_Primitive u64 = new BuiltIn_Primitive(PrimitiveType.U64);        
        
        public static readonly BuiltIn_Primitive _float = new BuiltIn_Primitive(PrimitiveType.Float);
        public static readonly BuiltIn_Primitive _double = new BuiltIn_Primitive(PrimitiveType.Double);
    }
}
