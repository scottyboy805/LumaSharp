
namespace LumaSharp.Runtime.Reflection
{
    public unsafe sealed class MetaAssembly
    {
        // Internal
        internal const int magic =  'L' |
                                    ('U' << 8) |
                                    ('V' << 16) |
                                    ('M' << 24);

        // Private
        private Stream assemblySource = null;

        private int token = 0;
        private MetaAssemblyName assemblyName = null;
        private MetaAssemblyName[] referenceNames = null;
        private MetaType[] types = null;

        // Properties
        public int Token
        {
            get { return token; }
        }

        public string Name
        {
            get { return assemblyName.Name; }
        }

        public MetaAssemblyName ModuleName
        {
            get { return assemblyName; }
        }

        public MetaAssemblyName[] ReferenceNames
        {
            get { return referenceNames; }
        }

        // Constructor
        internal MetaAssembly(Stream moduleSource, MetaAssemblyName moduleName)
        {
            this.assemblySource = moduleSource;
            this.assemblyName = moduleName;
        }

        // Methods
        public IEnumerable<MetaType> GetTypes(MemberFlags flags)
        {
            foreach(MetaType type in types)
            {
                // Check for member flags
                if (type.HasMemberFlags(flags) == true)
                    yield return type;
            }
        }

        public IEnumerable<MetaType> GetExportTypes()
        {
            foreach (MetaType type in types)
            {
                // Check for member flags
                if (type.IsExport == true)
                    yield return type;
            }
        }

        //internal void LoadMetadata(AppContext context, BinaryReader reader)
        //{
        //    // Read size
        //    int typeCount = reader.ReadInt32();

        //    // Initialize types
        //    types = new MetaType[typeCount];

        //    // Read all types
        //    for (int i = 0; i < typeCount; i++)
        //    {
        //        // Create type
        //        MetaType type = new MetaType(context);

        //        // Read type
        //        type.LoadTypeMetadata(reader);

        //        // Register type
        //        types[i] = type;
        //    }
        //}

        //internal void LoadExecutable(AppContext context, BinaryReader reader)
        //{
        //    // Read executable types
        //    for (int i = 0; i < types.Length; i++)
        //    {
        //        // Read the executable
        //        types[i].LoadTypeExecutable(reader);
        //    }
        //}

        internal static bool ReadModuleHeader(Stream stream, out MetaAssemblyName moduleName, bool throwOnError = true)
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
                moduleName = new MetaAssemblyName(reader, hintPath);
            }
            catch
            {
                // Check for throw
                if(throwOnError == true)
                    throw;

                moduleName = null;
                return false;
            }
            return true;
        }
    }
}
