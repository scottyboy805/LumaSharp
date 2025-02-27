
using System.Runtime.InteropServices;

namespace LumaSharp.Runtime.Reflection
{
    [Flags]
    public enum MetaTypeFlags : uint
    {
        Export = 1,
        Internal = 2,
        Hidden = 4,
        Global = 8,
        Type = 16,
        Contract = 32,
        Enum = 64,
        Array = 128,
        Abstract = 256,
        Override = 512,
        Generic = 1024,
    }

    public unsafe class MetaType : MetaMember
    {
        // Private
        private MetaTypeFlags typeFlags = 0;
        private RuntimeTypeCode typeCode = 0;
        private MetaType elementType = null;
        private MetaMember[] members = null;

        // Internal
        internal _TypeHandle* typeExecutable = null;

        // Properties
        public bool IsBuiltInType
        {
            get { return typeCode != RuntimeTypeCode.Any; }
        }

        public RuntimeTypeCode TypeCode
        {
            get { return typeCode; }
        }

        public bool IsType
        {
            get { return (typeFlags & MetaTypeFlags.Type) != 0; }
        }

        public bool IsContract
        {
            get { return (typeFlags & MetaTypeFlags.Contract) != 0; }
        }

        public bool IsEnum
        {
            get { return (typeFlags & MetaTypeFlags.Enum) != 0; }
        }

        public bool IsArray
        {
            get { return (typeFlags & MetaTypeFlags.Array) != 0; }
        }

        public bool IsGeneric
        {
            get { return (typeFlags & MetaTypeFlags.Generic) != 0; }
        }

        public MetaType ElementType
        {
            get { return elementType; }
        }

        // Constructor
        internal MetaType(AppContext context)
            : base(context)
        { 
        }

        protected MetaType(AppContext context, string name, RuntimeTypeCode code, MetaTypeFlags typeFlags)
            : base(context, name, (MemberFlags)typeFlags)
        {
            this.typeCode = code;
            this.typeFlags = typeFlags;
        }

        // Methods
        public IEnumerable<MetaMember> GetMembers(MemberFlags flags)
        {
            foreach(MetaMember member in members)
            {
                // Check for member flags
                if (member.HasMemberFlags(flags) == true)
                    yield return member;
            }
        }

        public IEnumerable<MetaType> GetTypes(MemberFlags flags)
        {
            foreach (MetaMember member in members)
            {
                // Check for field with flags
                if (member is MetaType && member.HasMemberFlags(flags) == true)
                    yield return (MetaType)member;
            }
        }

        public IEnumerable<MetaField> GetFields(MemberFlags flags)
        {
            foreach(MetaMember member in members)
            {
                // Check for field with flags
                if (member is MetaField && member.HasMemberFlags(flags) == true)
                    yield return (MetaField)member;
            }
        }

        public IEnumerable<MetaAccessor> GetAccessors(MemberFlags flags)
        {
            foreach (MetaMember member in members)
            {
                // Check for field with flags
                if (member is MetaAccessor && member.HasMemberFlags(flags) == true)
                    yield return (MetaAccessor)member;
            }
        }

        public IEnumerable<MetaMethod> GetInitializers(MemberFlags flags)
        {
            foreach (MetaMember member in members)
            {
                // Check for method with flags and not initializer
                if (member is MetaMethod && member.HasMemberFlags(flags) == true && ((MetaMethod)member).IsInitializer == true)
                    yield return (MetaMethod)member;
            }
        }

        public IEnumerable<MetaMethod> GetMethods(MemberFlags flags) 
        {
            foreach(MetaMember member in members)
            {
                // Check for method with flags and not initializer
                if(member is MetaMethod && member.HasMemberFlags(flags) == true && ((MetaMethod)member).IsInitializer == false)
                    yield return (MetaMethod)member;
            }
        }

        internal void LoadTypeMetadata(BinaryReader reader)
        {
            // Read member metadata
            LoadMemberMetadata(reader);

            // Get type flags
            typeFlags = (MetaTypeFlags)MemberFlags;

            List<MetaMember> members = new List<MetaMember>();

            // Read fields
            int fieldCount = reader.ReadInt32();

            // Read all fields
            for(int i = 0; i < fieldCount; i++)
            {
                // Create field
                MetaField field = new MetaField(context);

                // Read field
                field.LoadFieldMetadata(reader);

                // Register field
                members.Add(field);
            }

            // Read methods
            int methodCount = reader.ReadInt32();

            // Read all methods
            for(int i = 0; i < methodCount; i++)
            {
                // Create method
                MetaMethod method = new MetaMethod(context);

                // Read method
                method.LoadMethodMetadata(reader);

                // Register method
                members.Add(method);
            }

            // Store members
            this.members = members.ToArray();
        }

        internal void LoadTypeExecutable(BinaryReader reader)
        {
            // Create executable
            typeExecutable = (_TypeHandle*)NativeMemory.AllocZeroed((nuint)sizeof(_TypeHandle));

            // Read handle
            typeExecutable->Read(reader);


            // Read all fields
            foreach(MetaField field in members.OfType<MetaField>())
            {
                // Read executable
                field.LoadFieldExecutable(reader);
            }
        }
    }
}
