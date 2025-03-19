using System.Runtime.InteropServices;

namespace LumaSharp.Runtime.Handle
{
    public enum TokenKind : byte
    {
        Void = 0,

        AnyReference = 1,
        BoolReference,
        CharReference,
        I8Reference,
        U8Reference,
        I16Reference,
        U16Reference,
        I32Reference,
        U32Reference,
        I64Reference,
        U64Reference,
        F32Reference,
        F64Reference,
        PtrReference,
        UPtrReference,

        StringReference = 16,

        ModuleReference = 32,
        TypeReference,
        FieldReference,
        AccessorReference,
        MethodReference,

        ModuleDefinition = 64,
        TypeDefinition,
        FieldDefinition,
        AccessorDefinition,
        MethodDefinition,        
    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct _TokenHandle      // 4 byte meta token
    {
        // Private
        [FieldOffset(1)]
        private readonly byte moduleIndex;
        [FieldOffset(2)]
        private readonly ushort tokenIndex;

        // Public
        [FieldOffset(0)]
        public readonly TokenKind Kind;
        [FieldOffset(0)]
        public readonly int MetaToken;        

        // Properties
        /// <summary>
        /// Describes which reference assembly index the token points to, or zero if kind is a definition.
        /// </summary>
        internal byte ModuleIndex
        {
            get { return moduleIndex; }
        }

        /// <summary>
        /// Described the index of the referenced member (max members is 65565)
        /// </summary>
        internal ushort TokenIndex
        {
            get { return tokenIndex; }
        }

        // Constructor
        public _TokenHandle(int metaToken)
        {
            this.MetaToken = metaToken;
        }

        public _TokenHandle(RuntimeTypeCode typeCode)
        {
            this.Kind = (TokenKind)typeCode;
        }

        public _TokenHandle(TokenKind kind, byte moduleIndex, ushort tokenIndex)
        {
            this.Kind = kind;
            this.moduleIndex = moduleIndex;
            this.tokenIndex = tokenIndex;
        }

        // Methods
        public bool IsRuntimeType()
        {
            return Kind >= TokenKind.AnyReference &&
                Kind <= TokenKind.UPtrReference;
        }

        public static bool operator==(_TokenHandle a, _TokenHandle b)
        {
            return a.MetaToken == b.MetaToken;
        }

        public static bool operator!=(_TokenHandle a, _TokenHandle b)
        {
            return a.MetaToken != b.MetaToken;
        }
    }
}
