
namespace LumaSharp.Runtime
{
    public unsafe struct _StackHandle
    {
        // Internal
        internal _TypeHandle typeHandle;
        internal uint offset;

        // Methods
        public override string ToString()
        {
            return string.Format("_StackHandle(type = {0}, offset = {1}, size = {2})", typeHandle.TypeCode, offset, typeHandle.size);
        }
    }
}
