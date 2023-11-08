namespace LumaSharp.Runtime.Handle
{
    internal unsafe struct _MemoryHandle
    {
        // Internal
        internal int referenceCounter;
        internal _TypeHandle typeHandle;

        // Public
        public static readonly uint Size = (uint)sizeof(_MemoryHandle);
    }
}
