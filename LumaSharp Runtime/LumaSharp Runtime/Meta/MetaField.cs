
namespace LumaSharp.Runtime.Reflection
{
    [Flags]
    public enum FieldFlags : uint
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

        // Constructor
        internal MetaField(AppContext context)
            : base(context)
        { 
        }

        internal MetaField(AppContext context, string name, FieldFlags fieldFlags)
            : base(context, name, (MemberFlags)fieldFlags)
        {
            this.fieldFlags = fieldFlags;
        }

        // Methods
        internal void LoadFieldMetadata(BinaryReader reader)
        {
            //// Read member metadata
            //LoadMemberMetadata(reader);

            //// Get field flags
            //fieldFlags = (FieldFlags)MemberFlags;
        }

        internal void LoadFieldExecutable(BinaryReader reader)
        {
            //// Create executable
            //fieldExecutable = (_FieldHandle*)NativeMemory.Alloc((nuint)sizeof(_FieldHandle));

            //// Read handle
            //fieldExecutable->Read(reader);
        }
    }
}
