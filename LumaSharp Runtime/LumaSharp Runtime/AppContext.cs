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
        private Dictionary<int, MetaAssembly> loadedModules = new Dictionary<int, MetaAssembly>();

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
        public MetaAssembly LoadModule(string modulePath, bool metadataOnly = false)
        {
            // Check for null or empty
            if (string.IsNullOrEmpty(modulePath) == true)
                throw new ArgumentNullException(nameof(modulePath));

            // Check for not found 
            if (File.Exists(modulePath) == false)
                throw new ArgumentException("Could not find module path: " + modulePath);

            // Load the module
            return LoadModule(File.OpenRead(modulePath), metadataOnly);
        }

        public MetaAssembly LoadModule(Stream stream, bool metadataOnly = false)
        {
            // Check for null
            if(stream == null)
                throw new ArgumentNullException(nameof(stream));

            // Read the module header
            MetaAssemblyName moduleName;
            MetaAssembly.ReadModuleHeader(stream, out moduleName);

            // Check for already loaded
            MetaAssembly loadedModule = ResolveModule(moduleName.Name, moduleName.Version);

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
            loadedModules[module.Token] = module;
            return module;
        }

        public MetaAssembly ResolveModule(int token)
        {
            // Try to get the member
            MetaAssembly module;
            if (loadedModules.TryGetValue(token, out module) == false)
                throw new DllNotFoundException("Token: " + token);

            return module;
        }

        public MetaAssembly ResolveModule(string name)
        {
            return loadedModules.Values
                .FirstOrDefault(m => m.AssemblyName.Name == name);
        }

        public MetaAssembly ResolveModule(string name, Version version)
        {
            return loadedModules.Values
                .FirstOrDefault(m => m.AssemblyName.Name == name 
                && m.AssemblyName.Version.Equals(version));
        }

        public IEnumerable<MetaAssembly> GetLibraries()
        {
            return loadedModules.Values;
        }

        public void Dispose()
        {
        }

        internal ThreadContext GetCurrentThreadContext()
        {
            // Get the current thread
            return GetThreadContext(Thread.CurrentThread.ManagedThreadId);
        }

        internal unsafe ThreadContext GetThreadContext(int threadID, uint stackSize = 4096)
        {
            // Try to get
            ThreadContext context;
            if (threadContexts.TryGetValue(threadID, out context) == true)
                return context;

            // Create context
            context = new ThreadContext(this, stackSize);

            // Register context
            threadContexts[threadID] = context;

            // Get new context
            return context;
        }
    }
}
