
namespace LumaSharp.Runtime
{
    internal unsafe sealed class __runtime
    {
        // Public
        public const int maxTypeCode = (int)TypeCode.Double + 1;

        // Methods
        public static uint Size(int typeCode)
        {
            if (typeCode == 0)
                return 0;

            return Size((TypeCode)typeCode);
        }

        public static uint Size(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.I8: return sizeof(byte);
                case TypeCode.U8: return sizeof(sbyte);
                case TypeCode.I16: return sizeof(short);
                case TypeCode.U16: return sizeof(ushort);
                case TypeCode.I32: return sizeof(int);
                case TypeCode.U32: return sizeof(uint);
                case TypeCode.I64: return sizeof(long);
                case TypeCode.U64: return sizeof(ulong);
                case TypeCode.Float: return sizeof(float);
                case TypeCode.Double: return sizeof(double);
                case TypeCode.Char: return sizeof(char);
                case TypeCode.Bool: return sizeof(bool);
                case TypeCode.Any: return (uint)sizeof(IntPtr);
            }
            throw new NotSupportedException("Invalid type code: " + typeCode);
        }
    }
}
