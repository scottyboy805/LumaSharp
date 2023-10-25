
namespace LumaSharp.Runtime.Reflection
{
    public sealed class Library
    {
        // Private
        private int token = 0;
        private LibraryName libraryName = null;
        private LibraryName[] referenceNames = null;
        private Type[] types = null;

        // Properties
        public int Token
        {
            get { return token; }
        }

        public string Name
        {
            get { return libraryName.Name; }
        }

        public LibraryName LibraryName
        {
            get { return libraryName; }
        }

        public LibraryName[] ReferenceNames
        {
            get { return referenceNames; }
        }

        // Methods
        public IEnumerable<Type> GetTypes(MemberFlags flags)
        {
            foreach(Type type in types)
            {
                // Check for member flags
                if (type.HasMemberFlags(flags) == true)
                    yield return type;
            }
        }

        public IEnumerable<Type> GetExportTypes()
        {
            foreach (Type type in types)
            {
                // Check for member flags
                if (type.IsExport == true)
                    yield return type;
            }
        }
    }
}
