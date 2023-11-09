namespace LumaSharp.Runtime.Handle
{
    internal unsafe struct _MemoryHandle
    {
        // Internal
        internal int ReferenceCounter;
        internal _TypeHandle TypeHandle;

        // Public
        public static readonly uint Size = (uint)sizeof(_MemoryHandle);
    }
}
