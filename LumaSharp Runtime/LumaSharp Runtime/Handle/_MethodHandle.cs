
namespace LumaSharp.Runtime.Handle
{
    public unsafe struct _MethodHandle
    {
        // Internal
        internal int MethodToken;
        internal ushort MaxStack;             // Maximum size of the stack required for evaluation of the bytecode
        //internal ushort LocalHandleOffset;    // Offset into argLocals where local vars begin (after args)
        internal ushort ArgCount;
        internal ushort LocalCount;
        internal uint StackPtrOffset;           // Pointer offset from stack start where evaluation space starts (after args and locals)
        //internal _StackHandle[] ArgLocals;    // Contains argument followed by local type info
        //internal byte* InstructionPtr;        // Pointer to bytecode instruction set

        // Methods
        

        internal void Write(BinaryWriter writer)
        {
            writer.Write(MethodToken);
            writer.Write(MaxStack);
            writer.Write(ArgCount);
            writer.Write(LocalCount);
            writer.Write(StackPtrOffset);
        }

        internal void Read(BinaryReader reader)
        {
            MethodToken = reader.ReadInt32();
            MaxStack = reader.ReadUInt16();
            ArgCount = reader.ReadUInt16();
            LocalCount = reader.ReadUInt16();
            StackPtrOffset = reader.ReadUInt32();
        }
    }
}
