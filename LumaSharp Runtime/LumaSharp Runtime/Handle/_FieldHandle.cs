
namespace LumaSharp.Runtime.Handle
{
    public unsafe struct _FieldHandle
    {
        // Internal
        internal int FieldToken;
        internal uint FieldOffset;
        internal uint FieldSize;

        // Public
        public static readonly uint Size = (uint)sizeof(_FieldHandle);
    }
}
