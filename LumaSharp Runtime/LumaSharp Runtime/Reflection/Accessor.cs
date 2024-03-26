
namespace LumaSharp.Runtime.Reflection
{
    public class Accessor : Member
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
        private Type accessorType = null;
        private Method readMethod = null;
        private Method writeMethod = null;

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

        public Type AccessorType
        {
            get { return accessorType; }
        }

        public Method ReadMethod
        {
            get { return readMethod; }
        }

        public Method WriteMethod
        {
            get { return writeMethod; }
        }

        // Constructor
        internal Accessor(AppContext context, string name, AccessorFlags accessorFlags, MemberFlags memberFlags)
            : base(context, name, memberFlags)
        {
            this.accessorFlags = accessorFlags;
        }
    }
}
