namespace LumaSharp.Runtime.Handle
{
    internal unsafe struct _MemoryHandle
    {
        // Public
        public int ReferenceCounter;
        public readonly _TypeHandle* TypeHandle;

        // Public
        public static readonly uint Size = (uint)sizeof(_MemoryHandle);

        // Constructor
        internal _MemoryHandle(_TypeHandle* typeHandle)
        {
            this.ReferenceCounter = 0;
            this.TypeHandle = typeHandle;
        }
    }
}
