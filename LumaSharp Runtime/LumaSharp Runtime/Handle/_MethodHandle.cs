
namespace LumaSharp.Runtime.Handle
{
    public unsafe struct _MethodHandle
    {
        // Internal
        internal int methodToken;
        internal ushort maxStack;             // Maximum size of the stack required for evaluation of the bytecode
        internal ushort localHandleOffset;    // Offset into argLocals where local vars begin (after args)
        internal uint stackPtrOffset;         // Pointer offset from stack start where evaluation space starts (after args and locals)
        internal _StackHandle[] argLocals;    // Contains argument followed by local type info
        internal byte* instructionPtr;        // Pointer to bytecode instruction set
    }
}
