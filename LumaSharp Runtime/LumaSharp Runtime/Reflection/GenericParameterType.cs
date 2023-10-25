
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
        internal GenericParameterType(int index, TypeFlags typeFlags, MemberFlags memberFlags)
            : base(index.ToString(), TypeCode.Any, typeFlags, memberFlags)
        {
            this.genericParameterIndex = index;
        }
    }
}
