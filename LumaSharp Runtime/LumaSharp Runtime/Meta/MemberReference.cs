using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime.Reflection
{
    internal sealed class MemberReference<T> where T : MetaMember
    {
        // Private
        private AssemblyContext context = null;
        private _TokenHandle token = default;

        // Internal
        internal T resolvedMember = null;
        internal bool didResolveMember = false;

        // Properties
        public _TokenHandle Token
        {
            get { return token; }
        }

        public T Member
        {
            get
            {
                // Try to resolve
                if (resolvedMember == null && didResolveMember == false)
                {
                    resolvedMember = context.ResolveMember<T>(token);
                    didResolveMember = true;
                }

                // Get resolved member
                return resolvedMember;
            }
        }

        // Constructor
        public MemberReference(AssemblyContext context, _TokenHandle token)
        {
            this.context = context;
            this.token = token;
        }

        public MemberReference(MetaType fromType)
        {
            this.context = fromType.assemblyContext;
            this.token = fromType.Token;
            this.resolvedMember = fromType as T;
        }
    }
}
