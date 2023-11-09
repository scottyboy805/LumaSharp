
namespace LumaSharp.Runtime.Handle
{
    public unsafe struct _MethodHandle
    {
        // Internal
        internal int MethodToken;
        internal ushort MaxStack;             // Maximum size of the stack required for evaluation of the bytecode
        internal ushort LocalHandleOffset;    // Offset into argLocals where local vars begin (after args)
        internal uint StackPtrOffset;         // Pointer offset from stack start where evaluation space starts (after args and locals)
        internal _StackHandle[] ArgLocals;    // Contains argument followed by local type info
        internal byte* InstructionPtr;        // Pointer to bytecode instruction set
    }
}
