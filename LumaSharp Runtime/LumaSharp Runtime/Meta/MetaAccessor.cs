using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime.Reflection
{
    public class MetaAccessor : MetaMember
    {
        // Type
        protected internal enum AccessorFlags
        {
            Export = 1,
            Internal = 2,
            Hidden = 4,
            Global = 8,
            Abstract = 16,
            Override = 32,
            Generic = 64,
            Read = 128,
            Write = 256,
        }

        // Private
        private AccessorFlags accessorFlags = 0;
        private MetaType accessorType = null;
        private MetaMethod readMethod = null;
        private MetaMethod writeMethod = null;

        // Properties
        public bool IsAbstract
        {
            get { return (accessorFlags & AccessorFlags.Abstract) != 0; }
        }

        public bool IsOverride
        {
            get { return (accessorFlags & AccessorFlags.Override) != 0; }
        }

        public bool IsGeneric
        {
            get { return (accessorFlags & AccessorFlags.Generic) != 0; }
        }

        public bool HasRead
        {
            get { return (accessorFlags & AccessorFlags.Read) != 0; }
        }

        public bool HasWrite
        {
            get { return (accessorFlags & AccessorFlags.Write) != 0; }
        }

        public MetaType AccessorType
        {
            get { return accessorType; }
        }

        public MetaMethod ReadMethod
        {
            get { return readMethod; }
        }

        public MetaMethod WriteMethod
        {
            get { return writeMethod; }
        }

        // Constructor
        internal MetaAccessor(AssemblyContext context, _TokenHandle accessorToken, _TokenHandle declaringTypeToken, _TokenHandle nameToken, _TokenHandle accessorTypeToken, AccessorFlags accessorFlags)
            : base(context, accessorToken, declaringTypeToken, nameToken, (MetaMemberFlags)accessorFlags)
        {
            this.accessorFlags = accessorFlags;
        }
    }
}
