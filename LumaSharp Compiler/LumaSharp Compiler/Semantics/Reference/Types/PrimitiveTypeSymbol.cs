using LumaSharp.Runtime;
using LumaSharp.Compiler.AST;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Reference
{
    internal sealed class PrimitiveTypeSymbol : ITypeReferenceSymbol
    {
        // Private
        private string typeName = "";
        private AST.PrimitiveType primitiveType = 0;
        private ReferenceLibrary library = null;
        private ITypeReferenceSymbol[] baseTypes = null;

        private _TypeHandle typeHandle = default;

        // Properties
        public string TypeName => typeName;

        public string[] NamespaceName => null;

        public PrimitiveType PrimitiveType => primitiveType;

        public bool IsPrimitive => true;

        public bool IsType => false;

        public bool IsContract => false;

        public bool IsEnum => false;

        public INamespaceReferenceSymbol NamespaceSymbol => null;

        public ITypeReferenceSymbol DeclaringTypeSymbol => null;

        public IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols => null;

        public ITypeReferenceSymbol[] BaseTypeSymbols => baseTypes;

        public ITypeReferenceSymbol[] TypeMemberSymbols => null;

        public IFieldReferenceSymbol[] FieldMemberSymbols => null;

        public IAccessorReferenceSymbol[] AccessorMemberSymbols => null;

        public IMethodReferenceSymbol[] MethodMemberSymbols => null;

        public IMethodReferenceSymbol[] OperatorMemberSymbols => null;

        public ILibraryReferenceSymbol LibrarySymbol => library;

        public _TokenHandle SymbolToken => typeHandle.TypeToken;

        public string LibraryName => "runtime";

        public _TypeHandle TypeHandle => typeHandle;

        // Constructor
        internal PrimitiveTypeSymbol(ReferenceLibrary runtimeLibrary, PrimitiveType primitiveType, ITypeReferenceSymbol baseType = null)
        {
            this.library = runtimeLibrary;
            this.typeName = primitiveType.ToString().ToLower();
            this.primitiveType = primitiveType;

            if (baseType != null)
                this.baseTypes = new ITypeReferenceSymbol[] { baseType };

            // Create handle
            this.typeHandle = new _TypeHandle(new _TokenHandle((RuntimeTypeCode)primitiveType), GetRuntimeTypeSize(primitiveType));

            // Load members from reference library
        }

        internal PrimitiveTypeSymbol(ReferenceLibrary runtimeLibrary, string typeName, PrimitiveType primitiveType, ITypeReferenceSymbol baseType = null)
        {
            this.library = runtimeLibrary;
            this.typeName = typeName;
            this.primitiveType = primitiveType;

            if (baseType != null)
                this.baseTypes = new ITypeReferenceSymbol[] { baseType };

            // Load members from reference library
        }

        // Methods
        public override string ToString()
        {
            return TypeName;
        }

        private uint GetRuntimeTypeSize(PrimitiveType primitiveType)
        {
            return primitiveType switch
            {
                AST.PrimitiveType.Void => 0,
                AST.PrimitiveType.Any => RuntimeType.GetTypeSize(RuntimeTypeCode.Any),
                AST.PrimitiveType.Bool => RuntimeType.GetTypeSize(RuntimeTypeCode.Bool),
                AST.PrimitiveType.Char => RuntimeType.GetTypeSize(RuntimeTypeCode.Char),
                AST.PrimitiveType.I8 => RuntimeType.GetTypeSize(RuntimeTypeCode.I8),
                AST.PrimitiveType.U8 => RuntimeType.GetTypeSize(RuntimeTypeCode.U8),
                AST.PrimitiveType.I16 => RuntimeType.GetTypeSize(RuntimeTypeCode.I16),
                AST.PrimitiveType.U16 => RuntimeType.GetTypeSize(RuntimeTypeCode.U16),
                AST.PrimitiveType.I32 => RuntimeType.GetTypeSize(RuntimeTypeCode.I32),
                AST.PrimitiveType.U32 => RuntimeType.GetTypeSize(RuntimeTypeCode.U32),
                AST.PrimitiveType.I64 => RuntimeType.GetTypeSize(RuntimeTypeCode.I64),
                AST.PrimitiveType.U64 => RuntimeType.GetTypeSize(RuntimeTypeCode.U64),
                AST.PrimitiveType.F32 => RuntimeType.GetTypeSize(RuntimeTypeCode.F32),
                AST.PrimitiveType.F64 => RuntimeType.GetTypeSize(RuntimeTypeCode.F64),
                
                _ => throw new NotSupportedException(),
            };
        }
    }
}
