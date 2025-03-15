
using System.Runtime.CompilerServices;

namespace LumaSharp.Runtime.Handle
{
    internal unsafe readonly struct _ArrayHandle
    {
        // Public
        public readonly long ElementCount;
        public readonly _MemoryHandle MemoryHandle;

        // Public
        public static readonly uint Size = (uint)sizeof(_ArrayHandle);

        // Constructor
        internal _ArrayHandle(long elementCount, _TypeHandle* typeHandle)
        {
            this.ElementCount = elementCount;
            this.MemoryHandle = new _MemoryHandle(typeHandle);
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* GetElementAddress(byte* arr, long index)
        {
            return (byte*)(arr + (MemoryHandle.TypeHandle->TypeSize * index));
        }
    }
}
