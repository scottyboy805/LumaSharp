
namespace LumaSharp.Runtime.Handle
{
    internal unsafe struct _MethodHandle
    {
        // Public
        public int methodToken;
        public ushort maxStack;             // Maximum size of the stack required for evaluation of the bytecode
        public ushort localHandleOffset;    // Offset into argLocals where local vars begin (after args)
        public ushort argPtrOffset;         // Pointer offset from stack start where args are stored - usually 0 (before locals)
        public ushort localPtrOffset;       // Pointer offset from stack start where locals are stored (after args)
        public uint stackPtrOffset;     // Pointer offset from stack start where evaluation space starts (after args and locals)
        public _StackHandle[] argLocals;    // Contains argument followed by local type info
        public byte* instructionPtr;        // Pointer to bytecode instruction set
    }
}
