
namespace LumaSharp.Runtime.Reflection
{
    public class GenericMethod : MetaMethod
    {
        // Private
        private MetaType[] genericParameterTypes = null;

        // Properties
        public MetaType[] GenericParameterTypes
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
