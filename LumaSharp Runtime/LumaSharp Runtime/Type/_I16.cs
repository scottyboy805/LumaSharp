using System.Runtime.InteropServices;

namespace LumaSharp.Runtime
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct _I16
    {
        // Internal
        [FieldOffset(0)]
        internal short signed;
        [FieldOffset(0)]
        internal ushort unsigned;

        // Public
        public const int Size = sizeof(short);
    }
}
