using System.Runtime.InteropServices;

namespace LumaSharp.Runtime
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct _F32
    {
        // Internal
        [FieldOffset(0)]
        internal float data;

        // Public 
        public const int Size = sizeof(float);
    }
}
