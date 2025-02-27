
using LumaSharp.Runtime.Handle;
using System.Runtime.InteropServices;

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
        private FieldFlags fieldFlags = 0;
        private MetaType fieldType = null;

        // Internal
        internal _FieldHandle* fieldExecutable = null;

        // Properties
        public MetaType FieldType
        {
            get { return fieldType; }
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
            // Read member metadata
            LoadMemberMetadata(reader);

            // Get field flags
            fieldFlags = (FieldFlags)MemberFlags;
        }

        internal void LoadFieldExecutable(BinaryReader reader)
        {
            // Create executable
            fieldExecutable = (_FieldHandle*)NativeMemory.Alloc((nuint)sizeof(_FieldHandle));

            // Read handle
            fieldExecutable->Read(reader);
        }
    }
}
