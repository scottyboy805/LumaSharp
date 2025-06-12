using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime.Reflection;

namespace LumaSharp.Runtime.Reader
{
    internal enum AssemblyFlags : uint
    {
        None = 0,

        StringTable = 1 << 1,
        TypeDefTable = 1 << 2,
        TypeRefTable = 1 << 3,
        FieldDefTable = 1 << 4,
        FieldRefTable = 1 << 5,
        MethodDefTable = 1 << 6,
        MethodRefTable = 1 << 7,
    }

    internal struct AssemblyHeader
    {
        // Public
        public long TimeStamp;
        public AssemblyFlags Flags;
        public int MetadataOffset;
        public int MetadataSize;
        public int ExecutableOffset;
        public int ExecutableSize;
        public _TokenHandle EntryPoint;
    }

    public sealed class AssemblyReader
    {
        // Private
        private readonly string hintPath;
        private readonly Stream assemblyStream;
        private readonly BinaryReader assemblyReader;
        private readonly AssemblyHeader assemblyHeader;
        private readonly MetaAssemblyName assemblyName;
        private readonly MetaAssembly assembly;
        private readonly AssemblyContext assemblyContext;

        private readonly MetaReader metaReader;
        private readonly ExecutableReader executableReader;

        // Properties
        public string HintPath => hintPath;
        public Stream Stream => assemblyStream;
        public MetaAssemblyName AssemblyName => assemblyName;
        public MetaAssembly Assembly => assembly;
        public AssemblyContext AssemblyContext => assemblyContext;

        internal BinaryReader Reader => assemblyReader;
        internal AssemblyHeader Header => assemblyHeader;

        // Constructor
        public AssemblyReader(Stream assemblyStream, AppContext appContext, bool metadataOnly = false)
        {
            // Check for null
            if(assemblyStream == null)
                throw new ArgumentNullException(nameof(assemblyStream));

            // Get hint path
            this.hintPath = assemblyStream is FileStream fs ? fs.Name : null;

            this.assemblyStream = assemblyStream;
            this.assemblyReader = new BinaryReader(assemblyStream);
            this.assemblyHeader = ReadAssemblyHeader();
            this.assemblyName = ReadAssemblyName();

            // Create assembly
            this.assembly = new MetaAssembly(assemblyStream, assemblyName);

            // Create assembly context
            this.assemblyContext = new AssemblyContext(appContext, assembly);


            // Load metadata into context
            this.metaReader = new MetaReader(this);

            // Optionally load executable into context
            if(metadataOnly == false)
                this.executableReader = new ExecutableReader(this);
        }

        // Methods
        private AssemblyHeader ReadAssemblyHeader()
        {
            // Check for magic
            if (assemblyReader.ReadInt32() != MetaAssembly.magic)
                throw new InvalidDataException("Stream does not contain a valid Luma assembly format");

            // Create the header
            return new AssemblyHeader
            {
                TimeStamp = assemblyReader.ReadInt64(),
                Flags = (AssemblyFlags)assemblyReader.ReadUInt32(),
                MetadataOffset = assemblyReader.ReadInt32(),
                MetadataSize = assemblyReader.ReadInt32(),
                ExecutableOffset = assemblyReader.ReadInt32(),
                ExecutableSize = assemblyReader.ReadInt32(),
                EntryPoint = assemblyReader.ReadInt32(),
            };
        }

        private MetaAssemblyName ReadAssemblyName()
        {
            // Try to read name
            return new MetaAssemblyName(assemblyReader, hintPath);
        }
    }
}
