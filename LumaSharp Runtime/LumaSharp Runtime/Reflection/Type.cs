
namespace LumaSharp.Runtime.Reflection
{
    public unsafe class Type : Member
    {
        // Type
        protected internal enum TypeFlags
        {
            Type = 1,
            Contract = 2,
            Enum = 4,
            Array = 8,
            Generic = 16,
        }

        // Private
        private TypeFlags typeFlags = 0;
        private TypeCode typeCode = 0;
        private Type elementType = null;
        private Member[] members = null;

        private _TypeHandle* typeExecutable = null;

        // Properties
        public bool IsBuiltInType
        {
            get { return typeCode != TypeCode.Any; }
        }

        public TypeCode TypeCode
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
        protected Type(string name, TypeCode code, TypeFlags typeFlags, MemberFlags memberFlags)
            : base(name, memberFlags)
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
    }
}
