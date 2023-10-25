
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
        internal GenericType(string name, Type[] genericParameterTypes, TypeFlags typeFlags, MemberFlags memberFlags) 
            : base(name, TypeCode.Any, typeFlags | TypeFlags.Generic, memberFlags)
        {
            this.genericParameterTypes = genericParameterTypes;
        }
    }
}
