using System.Runtime.InteropServices;

namespace LumaSharp.Runtime
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct _I8
    {
        // Internal
        [FieldOffset(0)]
        internal sbyte signed;
        [FieldOffset(0)]
        internal byte unsigned;

        // Public
        public const int Size = sizeof(sbyte);
    }
}
