using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime.Reflection
{
    [Flags]
    public enum FieldFlags : ushort
    {
        Export = 1,
        Internal = 2,
        Hidden = 4,
        Global = 8,
        Constant = 16,
    }

    public unsafe class MetaField : MetaMember
    {
        // Private
        private readonly FieldFlags fieldFlags = 0;
        private readonly MemberReference<MetaType> fieldTypeReference = null;

        // Properties
        public MetaType FieldType
        {
            get { return fieldTypeReference.Member; }
        }

        public _TokenHandle FieldTypeToken
        {
            get { return fieldTypeReference.Token; }
        }

        // Constructor
        internal MetaField(AssemblyContext context, _TokenHandle fieldToken, _TokenHandle declaringTypeToken, _TokenHandle fieldNameToken, _TokenHandle fieldTypeToken, FieldFlags fieldFlags)
            : base(context, fieldToken, declaringTypeToken, fieldNameToken, (MetaMemberFlags)fieldFlags)
        {
            this.fieldFlags = fieldFlags;
            this.fieldTypeReference = new MemberReference<MetaType>(context, fieldTypeToken);
        }
    }
}
