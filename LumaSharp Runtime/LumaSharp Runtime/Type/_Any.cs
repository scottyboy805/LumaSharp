using System.Runtime.InteropServices;

namespace LumaSharp.Runtime
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct _Any
    {
        // Internal
        internal int typeToken;
        internal void* ptr;

        // Public
        public static readonly int Size = sizeof(_Any);
    }
}
