
namespace LumaSharp.Runtime
{
    public enum RuntimeTypeCode : byte
    {
        Void = 0,
        Any = 1,
        Bool,
        Char, 
        I8,
        U8,
        I16,
        U16,
        I32,
        U32,
        I64,
        U64,
        F32,
        F64,
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
                RuntimeTypeCode.Void => 0,

                RuntimeTypeCode.Any => (uint)IntPtr.Size,
                RuntimeTypeCode.Bool => sizeof(bool),
                RuntimeTypeCode.Char => sizeof(char),
                RuntimeTypeCode.I8 => sizeof(sbyte),
                RuntimeTypeCode.U8 => sizeof(byte),
                RuntimeTypeCode.I16 => sizeof(short),
                RuntimeTypeCode.U16 => sizeof(ushort),
                RuntimeTypeCode.I32 => sizeof(int),
                RuntimeTypeCode.U32 => sizeof(uint),
                RuntimeTypeCode.I64 => sizeof(long),
                RuntimeTypeCode.U64 => sizeof(ulong),
                RuntimeTypeCode.F32 => sizeof(float),
                RuntimeTypeCode.F64 => sizeof(double),
                RuntimeTypeCode.Ptr => (uint)IntPtr.Size,
                RuntimeTypeCode.UPtr => (uint)UIntPtr.Size,
                _ => throw new NotSupportedException(typeCode.ToString())
            };
        }
    }
}
