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
        internal GenericMethod(AssemblyContext context, _TokenHandle genericMethodToken, _TokenHandle declaringTypeToken, _TokenHandle nameToken, MetaMethodFlags methodFlags, _TokenHandle[] returnTypeTokens, MetaVariable[] parameters, int rva) 
            : base(context, genericMethodToken, declaringTypeToken, nameToken, methodFlags, returnTypeTokens, parameters, rva)
        {
        }
    }
}
