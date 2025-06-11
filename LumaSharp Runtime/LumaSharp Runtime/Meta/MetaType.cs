using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime.Reflection
{
    [Flags]
    public enum MetaTypeFlags : ushort
    {
        Export = 1 << 1,
        Internal = 1 << 2,
        Hidden = 1 << 3,
        Global =  1 << 4,
        Type = 1 << 5,
        Contract = 1 << 6,
        Enum = 1 << 7,
        Array = 1 << 8,
        Abstract = 1 << 9,
        Override = 1 << 10,
        Generic = 1 << 11,
        Copy = 1 << 12,
    }

    public unsafe class MetaType : MetaMember
    {
        // Private
        private readonly MetaTypeFlags typeFlags = 0;
        private readonly RuntimeTypeCode typeCode = 0;
        private readonly StringReference namespaceReference;
        private readonly MemberReference<MetaType> baseTypeReference;
        private readonly MemberReference<MetaType>[] contractTypeReferences;
        private readonly MemberReference<MetaMember>[] memberReferences;
        //private MetaType elementType = null;
        //private MetaMember[] members = null;

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

        public string Namespace
        {
            get { return namespaceReference.String; }
        }

        public _TokenHandle NamespaceToken
        {
            get { return namespaceReference.Token; }
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

        //public MetaType ElementType
        //{
        //    get { return elementType; }
        //}

        // Constructor
        internal MetaType(AssemblyContext context)
            : base(context)
        { 
        }

        internal MetaType(AssemblyContext context, _TokenHandle typeToken, _TokenHandle declaringTypeToken, _TokenHandle namespaceToken, _TokenHandle nameToken, RuntimeTypeCode code, MetaTypeFlags typeFlags)
            : base(context, typeToken, declaringTypeToken, nameToken, (MetaMemberFlags)typeFlags)
        {
            this.namespaceReference = new StringReference(context, namespaceToken);
            this.typeCode = code;
            this.typeFlags = typeFlags;            
        }

        internal MetaType(AssemblyContext context, _TokenHandle typeToken, _TokenHandle declaringTypeToken, _TokenHandle namespaceToken, _TokenHandle nameToken, RuntimeTypeCode code, MetaTypeFlags typeFlags, _TokenHandle baseTypeToken, _TokenHandle[] contractTypeTokens, _TokenHandle[] memberTokens)
            : base(context, typeToken, declaringTypeToken, nameToken, (MetaMemberFlags)typeFlags)
        {
            this.namespaceReference = new StringReference(context, namespaceToken);
            this.typeCode = code;
            this.typeFlags = typeFlags;
            this.baseTypeReference = new MemberReference<MetaType>(context, baseTypeToken);
            this.contractTypeReferences = contractTypeTokens.Select(c => new MemberReference<MetaType>(context, c)).ToArray();
            this.memberReferences = memberTokens.Select(m => new MemberReference<MetaMember>(context, m)).ToArray();
        }

        // Methods
        public IEnumerable<MetaMember> GetMembers(MetaMemberFlags flags)
        {
            foreach(MemberReference<MetaMember> memberReference in memberReferences)
            {
                // Check for member flags
                if (memberReference.Member.HasMemberFlags(flags) == true)
                    yield return memberReference.Member;
            }
        }

        public IEnumerable<MetaType> GetTypes(MetaMemberFlags flags)
        {
            foreach (MemberReference<MetaMember> memberReference in memberReferences)
            {
                // Check for field with flags
                if (memberReference.Member is MetaType && memberReference.Member.HasMemberFlags(flags) == true)
                    yield return (MetaType)memberReference.Member;
            }
        }

        public IEnumerable<MetaField> GetFields(MetaMemberFlags flags)
        {
            foreach (MemberReference<MetaMember> memberReference in memberReferences)
            {
                // Check for field with flags
                if (memberReference.Member is MetaField && memberReference.Member.HasMemberFlags(flags) == true)
                    yield return (MetaField)memberReference.Member;
            }
        }

        public IEnumerable<MetaAccessor> GetAccessors(MetaMemberFlags flags)
        {
            foreach (MemberReference<MetaMember> memberReference in memberReferences)
            {
                // Check for field with flags
                if (memberReference.Member is MetaAccessor && memberReference.Member.HasMemberFlags(flags) == true)
                    yield return (MetaAccessor)memberReference.Member;
            }
        }

        public IEnumerable<MetaMethod> GetInitializers(MetaMemberFlags flags)
        {
            foreach (MemberReference<MetaMember> memberReference in memberReferences)
            {
                // Check for method with flags and not initializer
                if (memberReference.Member is MetaMethod && memberReference.Member.HasMemberFlags(flags) == true && ((MetaMethod)memberReference.Member).IsInitializer == true)
                    yield return (MetaMethod)memberReference.Member;
            }
        }

        public IEnumerable<MetaMethod> GetMethods(MetaMemberFlags flags) 
        {
            foreach (MemberReference<MetaMember> memberReference in memberReferences)
            {
                // Check for method with flags and not initializer
                if(memberReference.Member is MetaMethod && memberReference.Member.HasMemberFlags(flags) == true && ((MetaMethod)memberReference.Member).IsInitializer == false)
                    yield return (MetaMethod)memberReference.Member;
            }
        }

        //internal void LoadTypeMetadata(BinaryReader reader)
        //{
        //    // Read member metadata
        //    LoadMemberMetadata(reader);

        //    // Get type flags
        //    typeFlags = (MetaTypeFlags)MemberFlags;

        //    List<MetaMember> members = new List<MetaMember>();

        //    // Read fields
        //    int fieldCount = reader.ReadInt32();

        //    // Read all fields
        //    for(int i = 0; i < fieldCount; i++)
        //    {
        //        // Create field
        //        MetaField field = new MetaField(context);

        //        // Read field
        //        field.LoadFieldMetadata(reader);

        //        // Register field
        //        members.Add(field);
        //    }

        //    // Read methods
        //    int methodCount = reader.ReadInt32();

        //    // Read all methods
        //    for(int i = 0; i < methodCount; i++)
        //    {
        //        // Create method
        //        MetaMethod method = new MetaMethod(context);

        //        // Read method
        //        method.LoadMethodMetadata(reader);

        //        // Register method
        //        members.Add(method);
        //    }

        //    // Store members
        //    this.members = members.ToArray();
        //}

        //internal void LoadTypeExecutable(BinaryReader reader)
        //{
        //    // Create executable
        //    typeExecutable = (_TypeHandle*)NativeMemory.AllocZeroed((nuint)sizeof(_TypeHandle));

        //    // Read handle
        //    typeExecutable->Read(reader);


        //    // Read all fields
        //    foreach(MetaField field in members.OfType<MetaField>())
        //    {
        //        // Read executable
        //        field.LoadFieldExecutable(reader);
        //    }
        //}
    }
}
