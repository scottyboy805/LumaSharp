using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LumaSharp_Compiler.Emit
{
    internal static class EmitUtil
    {
        // Methods
        public static void WriteStruct<T>(BinaryWriter writer, T data) where T : struct
        {
            // Create buffer
            byte[] buffer = new byte[Marshal.SizeOf<T>()];

            // Unsafe case
            Unsafe.As<byte, T>(ref buffer[0]) = data;

            // Write bytes
            writer.Write(buffer);
        }
    }
}
