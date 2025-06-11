
namespace LumaSharp.Runtime.Reflection
{
    public class GenericParameterType : MetaType
    {
        // Private
        private int genericParameterIndex = -1;

        // Properties
        public int GenericParameterIndex
        {
            get { return genericParameterIndex; }
        }

        // Constructor
        internal GenericParameterType(AssemblyContext context, int index, MetaTypeFlags typeFlags)
            : base(context, default, default, default, default, RuntimeTypeCode.Any, typeFlags)
        {
            this.genericParameterIndex = index;
        }
    }
}
