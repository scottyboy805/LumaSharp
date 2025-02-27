
namespace LumaSharp.Runtime.Reflection
{
    public class MetaAccessor : MetaMember
    {
        // Type
        protected internal enum AccessorFlags
        {
            Abstract = 2,
            Override = 4,
            Generic = 8,
            Read = 16,
            Write = 32,
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
        internal MetaAccessor(AppContext context, string name, AccessorFlags accessorFlags, MemberFlags memberFlags)
            : base(context, name, memberFlags)
        {
            this.accessorFlags = accessorFlags;
        }
    }
}
