
namespace LumaSharp.Runtime
{
    public unsafe struct _TypeHandle
    {
        // Internal
        internal int typeToken;
        internal uint size;

        // Properties
        public TypeCode TypeCode
        {
            get { return typeToken < __runtime.maxTypeCode ? (TypeCode)typeToken : 0; }
        }

        // Methods
        public override string ToString()
        {
            return string.Format("_TypeHandle(code = {0}, size = {1})", TypeCode, size);
        }
    }
}
