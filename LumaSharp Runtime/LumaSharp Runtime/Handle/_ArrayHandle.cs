
namespace LumaSharp.Runtime.Handle
{
    internal unsafe struct _ArrayHandle
    {
        // Internal
        internal uint elementCount;
        internal _MemoryHandle handle;

        // Public
        public static readonly uint Size = (uint)sizeof(_ArrayHandle);
    }
}
