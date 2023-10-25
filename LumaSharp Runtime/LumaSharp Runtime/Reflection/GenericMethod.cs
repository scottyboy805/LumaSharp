
namespace LumaSharp.Runtime.Reflection
{
    public class GenericMethod : Method
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
        internal GenericMethod(string name, MethodFlags methodFlags, MemberFlags memberFlags) 
            : base(name, methodFlags, memberFlags)
        {
        }
    }
}
