using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime.Reflection
{
    internal sealed class MemberReference<T> where T : MetaMember
    {
        // Private
        private AppContext context = null;
        private _TokenHandle symbolToken = default;

        // Internal
        internal T resolvedMember = null;
        internal bool didResolveMember = false;

        // Properties
        public _TokenHandle SymbolToken
        {
            get { return symbolToken; }
        }

        public T Member
        {
            get
            {
                // Try to resolve
                if (resolvedMember == null && didResolveMember == false)
                {
                    resolvedMember = context.ResolveMember<T>(symbolToken);
                    didResolveMember = true;
                }

                // Get resolved member
                return resolvedMember;
            }
        }

        // Constructor
        public MemberReference(AppContext context, _TokenHandle symbolToken)
        {
            this.context = context;
            this.symbolToken = symbolToken;
        }

        public MemberReference(MetaType fromType)
        {
            this.context = fromType.context;
            this.symbolToken = fromType.Token;
            this.resolvedMember = fromType as T;
        }
    }
}
