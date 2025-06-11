using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime.Reflection
{
    internal sealed class StringReference
    {
        // Private
        private AssemblyContext context = null;
        private _TokenHandle token = default;

        // Internal
        internal string resolvedString = null;
        internal bool didResolveString = false;

        // Properties
        public _TokenHandle Token
        {
            get { return token; }
        }

        public string String
        {
            get
            {
                // Try to resolve
                if (resolvedString == null && didResolveString == false)
                {
                    // Try to resolve
                    if (token.IsNil == false)
                    {
                        resolvedString = context.ResolveString(token);
                    }
                    didResolveString = true;
                }

                // Get resolved member
                return resolvedString;
            }
        }

        // Constructor
        public StringReference(AssemblyContext context, _TokenHandle token)
        {
            // Check for null
            if(context == null)
                throw new ArgumentNullException(nameof(context));

            // Check for string token
            if(token.Kind != TokenKind.StringReference)
                throw new ArgumentException(nameof(token) + " must be of kind: " + nameof(TokenKind.StringReference));

            this.context = context;
            this.token = token;
        }
    }
}
