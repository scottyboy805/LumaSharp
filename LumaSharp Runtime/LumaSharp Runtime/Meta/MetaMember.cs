using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime.Reflection
{
    [Flags]
    public enum MetaMemberFlags : ushort
    {
        Export = 1,
        Internal = 2,
        Hidden = 4,
        Global = 8,
    }

    public abstract class MetaMember
    {
        // Internal
        internal readonly AssemblyContext assemblyContext = null;

        // Private
        private readonly _TokenHandle memberToken;
        private readonly MemberReference<MetaType> declaringTypeReference;
        private readonly StringReference nameReference;
        private readonly MetaMemberFlags memberFlags;

        // Properties
        public _TokenHandle Token
        {
            get { return memberToken; }
        }

        public MetaType DeclaringType
        {
            get { return declaringTypeReference.Member; }
        }

        public _TokenHandle DeclaringTypeToken
        {
            get { return declaringTypeReference.Token; }
        }

        public string Name
        {
            get { return nameReference.String; }
        }

        public _TokenHandle NameToken
        {
            get { return nameReference.Token; }
        }

        public bool IsExport
        {
            get { return (memberFlags & MetaMemberFlags.Export) != 0; }
        }

        public bool IsInternal
        {
            get { return (memberFlags & MetaMemberFlags.Internal) != 0; }
        }

        public bool IsHidden
        {
            get { return (memberFlags & MetaMemberFlags.Hidden) != 0; }
        }

        public bool IsGlobal
        {
            get { return (memberFlags & MetaMemberFlags.Global) != 0; }
        }

        internal MetaMemberFlags MemberFlags
        {
            get { return memberFlags; }
        }

        // Constructor
        internal MetaMember(AssemblyContext context) 
        {
            this.assemblyContext = context;
        }

        protected MetaMember(AssemblyContext context, _TokenHandle token, _TokenHandle declaringTypeToken, _TokenHandle nameToken, MetaMemberFlags flags)
        {
            this.assemblyContext = context;
            this.memberToken = token;
            this.declaringTypeReference = new MemberReference<MetaType>(context, declaringTypeToken);
            this.nameReference = new StringReference(context, nameToken);
            this.memberFlags = flags;
        }

        // Methods
        public bool HasMemberFlags(MetaMemberFlags flags)
        {
            return (memberFlags & flags) == flags;
        }
    }
}
