
namespace LumaSharp.Runtime.Handle
{
    internal unsafe struct _ArrayHandle
    {
        // Internal
        internal uint ElementCount;
        internal _MemoryHandle MemoryHandle;

        // Public
        public static readonly uint Size = (uint)sizeof(_ArrayHandle);
    }
}
