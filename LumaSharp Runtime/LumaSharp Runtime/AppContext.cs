using LumaSharp.Runtime.Reflection;
using System.Runtime.InteropServices;

namespace LumaSharp.Runtime
{
    public unsafe sealed class AppContext : IDisposable
    {
        // Internal
        internal readonly _TypeHandle* primitivePtr = null;

        // Private
        private Dictionary<int, ThreadContext> threadContexts = new Dictionary<int, ThreadContext>();
        private Dictionary<string, MetaAssembly> loadedAssemblies = new Dictionary<string, MetaAssembly>();    // FullName, assembly

        // Constructor
        public AppContext()
        {
            // Allocate memory
            primitivePtr = (_TypeHandle*)NativeMemory.AllocZeroed((uint)sizeof(_TypeHandle) * (uint)RuntimeType.RuntimeTypeCodeSize + 1);

            // Init primitive types
            foreach(RuntimeTypeCode typeCode in Enum.GetValues<RuntimeTypeCode>())
            {
                primitivePtr[(int)typeCode] = new _TypeHandle(typeCode);
            }
        }

        ~AppContext()
        {
            NativeMemory.Free(primitivePtr);
        }

        // Methods
        public MetaAssembly LoadAssembly(string assemblyPath, bool metadataOnly = false)
        {
            // Check for null or empty
            if (string.IsNullOrEmpty(assemblyPath) == true)
                throw new ArgumentNullException(nameof(assemblyPath));

            // Check for not found 
            if (File.Exists(assemblyPath) == false)
                throw new ArgumentException("Could not find assembly path: " + assemblyPath);

            // Load the module
            return LoadAssembly(File.OpenRead(assemblyPath), metadataOnly);
        }

        public MetaAssembly LoadAssembly(Stream stream, bool metadataOnly = false)
        {
            // Check for null
            if(stream == null)
                throw new ArgumentNullException(nameof(stream));

            // Read the module header
            MetaAssemblyName moduleName;
            MetaAssembly.ReadModuleHeader(stream, out moduleName);

            // Check for already loaded
            MetaAssembly loadedModule = GetAssembly(moduleName.Name, moduleName.Version);

            // Get loaded module
            if (loadedModule != null)
                return loadedModule;


            // Create the module and reader
            MetaAssembly module = new MetaAssembly(stream, moduleName);
            BinaryReader reader = new BinaryReader(stream);

            //// Read module
            //module.LoadMetadata(this, reader);

            //// Check for executable
            //if(metadataOnly == false)
            //    module.LoadExecutable(this, reader);

            // Register the module
            loadedAssemblies[module.FullName] = module;
            return module;
        }

        public MetaAssembly GetAssembly(string name)
        {
            // Check arg
            if (string.IsNullOrEmpty(name) == true)
                throw new ArgumentException(nameof(name) + " cannot be null or empty");

            // Check for full name
            if (loadedAssemblies.TryGetValue(name, out MetaAssembly assembly) == true)
                return assembly;

            // Try to find
            return loadedAssemblies.Values
                .FirstOrDefault(m => m.AssemblyName.Name == name);
        }

        public MetaAssembly GetAssembly(string name, Version version)
        {
            // Check arg
            if (string.IsNullOrEmpty(name) == true)
                throw new ArgumentException(nameof(name) + " cannot be null or empty");

            // Check version
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            // Try to find
            return loadedAssemblies.Values
                .FirstOrDefault(m => m.AssemblyName.Name == name 
                && m.AssemblyName.Version.Equals(version));
        }

        public IEnumerable<MetaAssembly> GetAssemblies()
        {
            return loadedAssemblies.Values;
        }

        public void Dispose()
        {
        }

        internal ThreadContext GetCurrentThreadContext(uint stackSize = 4096)
        {
            // Get the current thread
            return GetThreadContext(Thread.CurrentThread.ManagedThreadId, stackSize);
        }

        internal unsafe ThreadContext GetThreadContext(int threadID, uint stackSize = 4096)
        {
            // Try to get
            ThreadContext context;
            if (threadContexts.TryGetValue(threadID, out context) == true)
                return context;

            // Create context
            context = new ThreadContext(stackSize);

            // Register context
            threadContexts[threadID] = context;

            // Get new context
            return context;
        }
    }
}
