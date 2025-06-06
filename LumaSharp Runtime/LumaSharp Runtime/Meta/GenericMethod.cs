using LumaSharp.Runtime.Handle;

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
        internal GenericMethod(AssemblyContext context, _TokenHandle token, string name, MetaMethodFlags methodFlags) 
            : base(context, token, name, methodFlags, null, null)
        {
        }
    }
}
