
using System.Runtime.CompilerServices;

namespace LumaSharp.Runtime.Handle
{
    internal unsafe struct _ArrayHandle
    {
        // Internal
        internal long ElementCount;
        internal _MemoryHandle MemoryHandle;

        // Public
        public static readonly uint Size = (uint)sizeof(_ArrayHandle);

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* GetElementAddress(byte* arr, long index)
        {
            return (byte*)(arr + (MemoryHandle.TypeHandle.TypeSize * index));
        }
    }
}
