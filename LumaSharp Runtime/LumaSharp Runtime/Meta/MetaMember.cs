using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime.Reflection
{
    [Flags]
    public enum MemberFlags : ushort
    {
        Export = 1,
        Internal = 2,
        Hidden = 4,
        Global = 8,
    }

    public abstract class MetaMember
    {
        // Internal
        internal AppContext context = null;

        // Private
        private _TokenHandle token = default;
        private string name = "";
        private MemberFlags memberFlags = 0;

        // Properties
        public _TokenHandle Token
        {
            get { return token; }
        }

        public string Name
        {
            get { return name; }
        }

        public bool IsExport
        {
            get { return (memberFlags & MemberFlags.Export) != 0; }
        }

        public bool IsInternal
        {
            get { return (memberFlags & MemberFlags.Internal) != 0; }
        }

        public bool IsHidden
        {
            get { return (memberFlags & MemberFlags.Hidden) != 0; }
        }

        public bool IsGlobal
        {
            get { return (memberFlags & MemberFlags.Global) != 0; }
        }

        internal MemberFlags MemberFlags
        {
            get { return memberFlags; }
        }

        // Constructor
        internal MetaMember(AppContext context) 
        {
            this.context = context;
        }

        protected MetaMember(AppContext context, _TokenHandle token, string name, MemberFlags flags)
        {
            this.context =context;
            this.token = token;
            this.name = name;
            this.memberFlags = flags;
        }

        // Methods
        public bool HasMemberFlags(MemberFlags flags)
        {
            return (memberFlags & flags) == flags;
        }

        //internal void LoadMemberMetadata(BinaryReader reader)
        //{
        //    metaToken = new _TokenHandle(reader.ReadInt32());
        //    name = reader.ReadString();
        //    memberFlags = (MemberFlags)reader.ReadUInt32();
        //}
    }
}
