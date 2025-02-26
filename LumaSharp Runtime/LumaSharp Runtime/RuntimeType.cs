
namespace LumaSharp.Runtime
{
    public enum RuntimeTypeCode : byte
    {
        Any = 1,
        Bool,
        Char, 
        I1,
        U1,
        I2,
        U2,
        I4,
        U4,
        I8,
        U8,
        F4,
        F8,
        Ptr,
        UPtr,
    }

    public static class RuntimeType
    {
        // Public
        public static readonly int RuntimeTypeCodeSize = Enum.GetNames(typeof(RuntimeTypeCode)).Length;

        // Methods
        public static uint GetTypeSize(RuntimeTypeCode typeCode)
        {
            return typeCode switch
            {
                RuntimeTypeCode.Any => (uint)IntPtr.Size,
                RuntimeTypeCode.Bool => sizeof(bool),
                RuntimeTypeCode.Char => sizeof(char),
                RuntimeTypeCode.I1 => sizeof(sbyte),
                RuntimeTypeCode.U1 => sizeof(byte),
                RuntimeTypeCode.I2 => sizeof(short),
                RuntimeTypeCode.U2 => sizeof(ushort),
                RuntimeTypeCode.I4 => sizeof(int),
                RuntimeTypeCode.U4 => sizeof(uint),
                RuntimeTypeCode.I8 => sizeof(long),
                RuntimeTypeCode.U8 => sizeof(ulong),
                RuntimeTypeCode.F4 => sizeof(float),
                RuntimeTypeCode.F8 => sizeof(double),
                RuntimeTypeCode.Ptr => (uint)IntPtr.Size,
                RuntimeTypeCode.UPtr => (uint)UIntPtr.Size,
                _ => throw new NotSupportedException(typeCode.ToString())
            };
        }
    }
}
