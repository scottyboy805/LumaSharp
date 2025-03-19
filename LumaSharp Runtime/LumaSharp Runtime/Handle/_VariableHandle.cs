
namespace LumaSharp.Runtime
{
    [Flags]
    public enum _VariableFlags : uint
    {
        ByRef = 1 << 0,
        Optional = 1 << 1,
    }

    public unsafe readonly struct _VariableHandle
    {
        // Public
        public readonly _TypeHandle TypeHandle;
        public readonly uint StackOffset;

        // Constructor
        public _VariableHandle(_TypeHandle typeHandle, uint stackOffset)
        {
            this.TypeHandle = typeHandle;
            this.StackOffset = stackOffset;
        }

        public _VariableHandle(RuntimeTypeCode typeCode, uint stackOffset)
        {
            this.TypeHandle = new _TypeHandle(typeCode);
            this.StackOffset = stackOffset;
        }

        // Methods
        public override string ToString()
        {
            return string.Format("_VariableHandle(type = {0}, offset = {1}, size = {2})", TypeHandle.TypeCode, StackOffset, TypeHandle.TypeSize);
        }
    }
}
