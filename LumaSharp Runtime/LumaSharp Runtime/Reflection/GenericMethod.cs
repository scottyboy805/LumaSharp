
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
        internal GenericMethod(AppContext context, string name, MethodFlags methodFlags) 
            : base(context, name, methodFlags)
        {
        }
    }
}
