
namespace LumaSharp.Runtime.Reflection
{
    public enum MemberFlags
    {
        Export = 1,
        Internal = 2,
        Hidden = 4,
        Global = 8,
    }

    public abstract class Member
    {
        // Private
        private int token = 0;
        private string name = "";
        private MemberFlags memberFlags = 0;

        // Properties
        public int Token
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

        // Constructor
        protected Member(string name, MemberFlags flags)
        {
            this.name = name;
            this.memberFlags = flags;
        }

        // Methods
        public bool HasMemberFlags(MemberFlags flags)
        {
            return (memberFlags & flags) == flags;
        }
    }
}
