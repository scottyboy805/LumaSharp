using System.Runtime.InteropServices;

namespace LumaSharp.Runtime
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct _I32
    {
        // Internal
        [FieldOffset(0)]
        internal int signed;
        [FieldOffset(0)]
        internal uint unsigned;
        
        // Public
        public const int Size = sizeof(int);
    }
}
