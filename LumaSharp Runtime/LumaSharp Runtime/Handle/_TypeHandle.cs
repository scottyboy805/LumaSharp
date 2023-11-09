
namespace LumaSharp.Runtime
{
    public unsafe struct _TypeHandle
    {
        // Internal
        internal int TypeToken;
        internal uint TypeSize;

        // Properties
        public TypeCode TypeCode
        {
            get { return TypeToken < __runtime.maxTypeCode ? (TypeCode)TypeToken : 0; }
        }

        // Methods
        public override string ToString()
        {
            return string.Format("_TypeHandle(code = {0}, size = {1})", TypeCode, TypeSize);
        }
    }
}
