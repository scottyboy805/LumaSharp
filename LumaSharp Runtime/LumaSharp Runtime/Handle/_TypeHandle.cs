
using System.Runtime.InteropServices;

namespace LumaSharp.Runtime
{
    public unsafe readonly struct _TypeHandle
    {
        // Public
        public readonly int TypeToken;
        public readonly uint TypeSize;

        // Properties
        public RuntimeTypeCode TypeCode
        {
            get { return TypeToken < RuntimeType.RuntimeTypeCodeSize ? (RuntimeTypeCode)TypeToken : 0; }
        }

        // Constructor
        public _TypeHandle(RuntimeTypeCode typeCode)
        {
            this.TypeToken = (int)typeCode;
            this.TypeSize = RuntimeType.GetTypeSize(typeCode);
        }

        // Methods
        public override string ToString()
        {
            return string.Format("_TypeHandle(code = {0}, size = {1})", TypeCode, TypeSize);
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(TypeToken);
            writer.Write(TypeSize);
        }

        internal void Read(BinaryReader reader)
        {
            //TypeToken = reader.ReadInt32();
            //TypeSize = reader.ReadUInt32();
        }
    }
}
