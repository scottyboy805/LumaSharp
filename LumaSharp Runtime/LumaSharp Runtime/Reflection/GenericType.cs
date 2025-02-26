
namespace LumaSharp.Runtime.Reflection
{
    public class GenericType : Type
    {
        // Private
        private Type[] genericParameterTypes = null;        

        // Properties
        public Type[] GenericParameterTypes
        {
            get { return genericParameterTypes; }
        }

        public int GenericParameterCount
        {
            get { return genericParameterTypes.Length; }
        }

        // Constructor
        internal GenericType(AppContext context, string name, Type[] genericParameterTypes, TypeFlags typeFlags) 
            : base(context, name, RuntimeTypeCode.Any, typeFlags | TypeFlags.Generic)
        {
            this.genericParameterTypes = genericParameterTypes;
        }
    }
}
