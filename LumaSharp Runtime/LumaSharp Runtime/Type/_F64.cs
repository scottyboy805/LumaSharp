using System.Runtime.InteropServices;

namespace LumaSharp.Runtime
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct _F64
    {
        // Internal
        [FieldOffset(0)]
        internal double data;

        // Public
        public const int Size = sizeof(double);
    }
}
