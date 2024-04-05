
namespace LumaSharp.Runtime.Reflection
{
    public unsafe sealed class Module
    {
        // Internal
        internal const int magic =  'L' |
                                    ('U' << 8) |
                                    ('V' << 16) |
                                    ('M' << 24);

        // Private
        private Stream moduleSource = null;

        private int token = 0;
        private ModuleName moduleName = null;
        private ModuleName[] referenceNames = null;
        private Type[] types = null;

        // Properties
        public int Token
        {
            get { return token; }
        }

        public string Name
        {
            get { return moduleName.Name; }
        }

        public ModuleName ModuleName
        {
            get { return moduleName; }
        }

        public ModuleName[] ReferenceNames
        {
            get { return referenceNames; }
        }

        // Constructor
        internal Module(Stream moduleSource, ModuleName moduleName)
        {
            this.moduleSource = moduleSource;
            this.moduleName = moduleName;
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

        internal void LoadMetadata(AppContext context, BinaryReader reader)
        {
            // Read size
            int typeCount = reader.ReadInt32();

            // Initialize types
            types = new Type[typeCount];

            // Read all types
            for (int i = 0; i < typeCount; i++)
            {
                // Create type
                Type type = new Type(context);

                // Read type
                type.LoadTypeMetadata(reader);

                // Register type
                types[i] = type;
            }
        }

        internal void LoadExecutable(AppContext context, BinaryReader reader)
        {
            // Read executable types
            for (int i = 0; i < types.Length; i++)
            {
                // Read the executable
                types[i].LoadTypeExecutable(reader);
            }
        }

        internal static bool ReadModuleHeader(Stream stream, out ModuleName moduleName, bool throwOnError = true)
        {
            string hintPath = null;

            // Check for path available
            if (stream is FileStream)
                hintPath = ((FileStream)stream).Name;

            // Create reader
            BinaryReader reader = new BinaryReader(stream);

            // Check for magic
            if(reader.ReadInt32() != magic)
            {
                // Check for throw
                if (throwOnError == true)
                    throw new InvalidDataException("Stream does not contain a valid Luma module format");

                moduleName = null;
                return false;
            }

            try
            {
                // Create module name
                moduleName = new ModuleName(reader, hintPath);
            }
            catch(Exception e)
            {
                // Check for throw
                if(throwOnError == true)
                    throw e;

                moduleName = null;
                return false;
            }
            return true;
        }
    }
}
