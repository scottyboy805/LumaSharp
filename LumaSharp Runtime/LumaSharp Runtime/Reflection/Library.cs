
namespace LumaSharp.Runtime.Reflection
{
    public unsafe sealed class Library
    {
        // Internal
        internal const int magic =  'L' |
                                    ('U' << 8) |
                                    ('V' << 16) |
                                    ('M' << 24);

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

        private void LoadMetadata(AppContext context, BinaryReader reader)
        {
            // Check for luma vm image
            if (reader.ReadInt32() != magic)
                throw new InvalidDataException("Not a valid library format");

            // Read name
            libraryName = new LibraryName(
                reader.ReadString(),
                new Version());

            // Read meta types
            LoadMetadataTypes(context, reader);
        }

        private void LoadMetadataTypes(AppContext context, BinaryReader reader)
        {
            // Read size
            int typeCount = reader.ReadInt32();

            // Initialize types
            types = new Type[typeCount];

            // Read all types
            for(int i = 0; i <  typeCount; i++)
            {
                // Create type
                Type type = new Type(context);

                // Read type
                type.LoadTypeMetadata(reader);

                // Register type
                types[i] = type;
            }
        }

        private void LoadExecutableTypes(BinaryReader reader)
        {
            // Read executable types
            for(int i = 0; i < types.Length; i++)
            {
                // Read the executable
                types[i].LoadTypeExecutable(reader);
            }
        }
    }
}
