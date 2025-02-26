
namespace LumaSharp.Runtime.Reflection
{
    public class GenericParameterType : Type
    {
        // Private
        private int genericParameterIndex = -1;

        // Properties
        public int GenericParameterIndex
        {
            get { return genericParameterIndex; }
        }

        // Constructor
        internal GenericParameterType(AppContext context, int index, TypeFlags typeFlags)
            : base(context, index.ToString(), RuntimeTypeCode.Any, typeFlags)
        {
            this.genericParameterIndex = index;
        }
    }
}
