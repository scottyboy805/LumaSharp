
namespace LumaSharp.Runtime.Reflection
{
    internal sealed unsafe class PrimitiveType : Type
    {
        // Constructor
        internal PrimitiveType(AppContext context, RuntimeTypeCode code, _TypeHandle* typeHandle) 
            : base(context, code.ToString(), code, 0)
        {
            this.typeExecutable = typeHandle;
        }
    }
}
