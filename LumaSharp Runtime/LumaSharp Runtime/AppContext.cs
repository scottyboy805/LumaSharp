using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime.Reflection;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MetaType = LumaSharp.Runtime.Reflection.MetaType;

namespace LumaSharp.Runtime
{
    public unsafe sealed class AppContext : IDisposable
    {
        // Internal
        internal _TypeHandle* _anyType = null;
        internal readonly ConcurrentDictionary<_TokenHandle, IntPtr> typeHandles = new();        // token, _TypeHandle*
        internal readonly ConcurrentDictionary<_TokenHandle, IntPtr> fieldHandles = new();       // token, _FieldHandle*
        internal readonly ConcurrentDictionary<_TokenHandle, IntPtr> methodHandles = new();      // token, _MethodHandle*
        internal readonly ConcurrentDictionary<_TokenHandle, IntPtr> globalMemoryHandles = new();

        // Private
        private _TypeHandle* primitivePtr = null;
        private Dictionary<int, ThreadContext> threadContexts = new Dictionary<int, ThreadContext>();
        private Dictionary<int, MetaAssembly> loadedModules = new Dictionary<int, MetaAssembly>();
        private Dictionary<_TokenHandle, MetaType> loadedTypes = new();
        private Dictionary<_TokenHandle, MetaMember> loadedMembers = new();       


        // Constructor
        public AppContext()
        {
            // Allocate memory
            primitivePtr = (_TypeHandle*)NativeMemory.AllocZeroed((uint)sizeof(_TypeHandle) * (uint)RuntimeType.RuntimeTypeCodeSize + 1);

            // Init primitive types
            foreach(RuntimeTypeCode typeCode in Enum.GetValues<RuntimeTypeCode>())
            {
                primitivePtr[(int)typeCode] = new _TypeHandle(typeCode);
                typeHandles[typeCode] = (IntPtr)(&primitivePtr[(int)typeCode]);
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
                .FirstOrDefault(m => m.ModuleName.Name == name);
        }

        public MetaAssembly ResolveModule(string name, Version version)
        {
            return loadedModules.Values
                .FirstOrDefault(m => m.ModuleName.Name == name 
                && m.ModuleName.Version.Equals(version));
        }

        public MetaMember ResolveMember(_TokenHandle token)
        {
            // Try to get the member
            MetaMember member;
            if (loadedMembers.TryGetValue(token, out member) == true)
                return member;

            // Check for type
            MetaType type;
            if (loadedTypes.TryGetValue(token, out type) == true)
                return type;

            // Not found
            throw new MissingMemberException("Token: " + token);
        }

        public T ResolveMember<T>(_TokenHandle token) where T : MetaMember
        {
            return ResolveMember(token) as T;
        }

        public MetaType ResolveType(_TokenHandle token)
        {
            throw new NotImplementedException();
        }

        public MetaField ResolveField(_TokenHandle token)
        {
            // Try to get the member
            MetaMember member;
            if (loadedMembers.TryGetValue(token, out member) == false || (member is MetaField) == false)
                throw new MissingMemberException("Token: " + token);

            return member as MetaField;
        }

        public MetaAccessor ResolveAccessor(_TokenHandle token)
        {
            // Try to get the member
            MetaMember member;
            if (loadedMembers.TryGetValue(token, out member) == false || (member is MetaAccessor) == false)
                throw new MissingMemberException("Token: " + token);

            return member as MetaAccessor;
        }

        public MetaMethod ResolveMethod(_TokenHandle token)
        {
            // Try to get the member
            MetaMember member;
            if (loadedMembers.TryGetValue(token, out member) == false || (member is MetaMethod) == false)
                throw new MissingMemberException("Token: " + token);

            return member as MetaMethod;
        }

        public IEnumerable<MetaAssembly> GetLibraries()
        {
            return loadedModules.Values;
        }

        public IEnumerable<MetaType> GetTypes(MemberFlags flags)
        {
            foreach(MetaType type in loadedTypes.Values)
            {
                // Check for field with flags
                if (type.HasMemberFlags(flags) == true)
                    yield return type;
            }
        }

        public void Dispose()
        {
        }

        internal void DefineMetaMember(MetaMember member)
        {
            // Check for already added
            if (loadedMembers.ContainsKey(member.Token) == true)
                throw new InvalidOperationException("Member with token already exists: " + member.Token);

            // Store member
            loadedMembers[member.Token] = member;
        }

        internal void DefineExecutableMethod(_MethodHandle* method)
        {
            // Check for already added
            if (methodHandles.ContainsKey(method->MethodToken) == true)
                throw new InvalidOperationException("Method executable with token already exists: " + method->MethodToken);

            // Store method
            methodHandles[method->MethodToken] = (IntPtr)method;
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
