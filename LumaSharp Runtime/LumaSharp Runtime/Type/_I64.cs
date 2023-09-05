using System.Runtime.InteropServices;

namespace LumaSharp.Runtime
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct _I64
    {
        // Internal
        [FieldOffset(0)]
        internal long signed;
        [FieldOffset(0)]
        internal ulong unsigned;

        // Public
        public const int Size = sizeof(long);
    }
}
