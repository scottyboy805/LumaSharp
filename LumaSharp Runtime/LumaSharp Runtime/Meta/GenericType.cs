
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime.Reflection
{
    public class GenericType : MetaType
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
        internal GenericType(AssemblyContext context, _TokenHandle genericTypeToken, _TokenHandle declaringTypeToken, _TokenHandle namespaceToken, _TokenHandle nameToken, MetaType[] genericParameterTypes, MetaTypeFlags typeFlags) 
            : base(context, genericTypeToken, declaringTypeToken, namespaceToken, nameToken, RuntimeTypeCode.Any, typeFlags | MetaTypeFlags.Generic)
        {
            this.genericParameterTypes = genericParameterTypes;
        }
    }
}
