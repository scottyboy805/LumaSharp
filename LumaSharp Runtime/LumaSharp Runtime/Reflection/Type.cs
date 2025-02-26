
using System.Runtime.InteropServices;

namespace LumaSharp.Runtime.Reflection
{
    [Flags]
    public enum TypeFlags : uint
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

    public unsafe class Type : Member
    {
        // Private
        private TypeFlags typeFlags = 0;
        private RuntimeTypeCode typeCode = 0;
        private Type elementType = null;
        private Member[] members = null;

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
            get { return (typeFlags & TypeFlags.Type) != 0; }
        }

        public bool IsContract
        {
            get { return (typeFlags & TypeFlags.Contract) != 0; }
        }

        public bool IsEnum
        {
            get { return (typeFlags & TypeFlags.Enum) != 0; }
        }

        public bool IsArray
        {
            get { return (typeFlags & TypeFlags.Array) != 0; }
        }

        public bool IsGeneric
        {
            get { return (typeFlags & TypeFlags.Generic) != 0; }
        }

        public Type ElementType
        {
            get { return elementType; }
        }

        // Constructor
        internal Type(AppContext context)
            : base(context)
        { 
        }

        protected Type(AppContext context, string name, RuntimeTypeCode code, TypeFlags typeFlags)
            : base(context, name, (MemberFlags)typeFlags)
        {
            this.typeCode = code;
            this.typeFlags = typeFlags;
        }

        // Methods
        public IEnumerable<Member> GetMembers(MemberFlags flags)
        {
            foreach(Member member in members)
            {
                // Check for member flags
                if (member.HasMemberFlags(flags) == true)
                    yield return member;
            }
        }

        public IEnumerable<Type> GetTypes(MemberFlags flags)
        {
            foreach (Member member in members)
            {
                // Check for field with flags
                if (member is Type && member.HasMemberFlags(flags) == true)
                    yield return (Type)member;
            }
        }

        public IEnumerable<Field> GetFields(MemberFlags flags)
        {
            foreach(Member member in members)
            {
                // Check for field with flags
                if (member is Field && member.HasMemberFlags(flags) == true)
                    yield return (Field)member;
            }
        }

        public IEnumerable<Accessor> GetAccessors(MemberFlags flags)
        {
            foreach (Member member in members)
            {
                // Check for field with flags
                if (member is Accessor && member.HasMemberFlags(flags) == true)
                    yield return (Accessor)member;
            }
        }

        public IEnumerable<Method> GetInitializers(MemberFlags flags)
        {
            foreach (Member member in members)
            {
                // Check for method with flags and not initializer
                if (member is Method && member.HasMemberFlags(flags) == true && ((Method)member).IsInitializer == true)
                    yield return (Method)member;
            }
        }

        public IEnumerable<Method> GetMethods(MemberFlags flags) 
        {
            foreach(Member member in members)
            {
                // Check for method with flags and not initializer
                if(member is Method && member.HasMemberFlags(flags) == true && ((Method)member).IsInitializer == false)
                    yield return (Method)member;
            }
        }

        internal void LoadTypeMetadata(BinaryReader reader)
        {
            // Read member metadata
            LoadMemberMetadata(reader);

            // Get type flags
            typeFlags = (TypeFlags)MemberFlags;

            List<Member> members = new List<Member>();

            // Read fields
            int fieldCount = reader.ReadInt32();

            // Read all fields
            for(int i = 0; i < fieldCount; i++)
            {
                // Create field
                Field field = new Field(context);

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
                Method method = new Method(context);

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
            foreach(Field field in members.OfType<Field>())
            {
                // Read executable
                field.LoadFieldExecutable(reader);
            }
        }
    }
}
