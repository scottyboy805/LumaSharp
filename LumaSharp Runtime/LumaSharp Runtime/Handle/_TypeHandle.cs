using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime
{
    public unsafe readonly struct _TypeHandle
    {
        // Public
        public readonly _TokenHandle TypeToken;
        public readonly uint TypeSize;

        // Properties
        public RuntimeTypeCode TypeCode
        {
            get { return TypeToken.IsRuntimeType() ? (RuntimeTypeCode)TypeToken.Kind : 0; }
        }

        // Constructor
        public _TypeHandle(_TokenHandle typeToken, uint typeSize)
        {
            this.TypeToken = typeToken;
            this.TypeSize = typeSize;
        }

        public _TypeHandle(RuntimeTypeCode typeCode)
        {
            this.TypeToken = new _TokenHandle((int)typeCode);
            this.TypeSize = RuntimeType.GetTypeSize(typeCode);
        }

        // Methods
        public override string ToString()
        {
            return string.Format("_TypeHandle(code = {0}, size = {1})", TypeCode, TypeSize);
        }
    }
}
