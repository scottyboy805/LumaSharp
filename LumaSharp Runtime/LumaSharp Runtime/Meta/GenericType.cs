
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
        internal GenericType(AppContext context, string name, MetaType[] genericParameterTypes, MetaTypeFlags typeFlags) 
            : base(context, name, RuntimeTypeCode.Any, typeFlags | MetaTypeFlags.Generic)
        {
            this.genericParameterTypes = genericParameterTypes;
        }
    }
}
