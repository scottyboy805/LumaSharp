using System.Reflection.Metadata;

namespace LumaSharp.Runtime.Reflection
{
    internal sealed class MemberReference<T> where T : Member
    {
        // Private
        private AppContext context = null;
        private int symbolToken = -1;

        // Internal
        internal T resolvedMember = null;
        internal bool didResolveMember = true;

        // Properties
        public int SymbolToken
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
        public MemberReference(AppContext context, int symbolToken)
        {
            this.context = context;
            this.symbolToken = symbolToken;
        }

        public MemberReference(Type fromType)
        {
            if ((typeof(T) is Type) == false)
                throw new InvalidOperationException("Only type is supported");

            this.context = fromType.context;
            this.symbolToken = fromType.Token;
            this.resolvedMember = fromType as T;
        }
    }
}
