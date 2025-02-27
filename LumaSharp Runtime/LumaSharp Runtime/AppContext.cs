using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime.Reflection;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Type = LumaSharp.Runtime.Reflection.Type;

namespace LumaSharp.Runtime
{
    public unsafe sealed class AppContext : IDisposable
    {
        // Internal
        internal _TypeHandle* _anyType = null;
        internal readonly ConcurrentDictionary<int, IntPtr> typeHandles = new();        // token, _TypeHandle*
        internal readonly ConcurrentDictionary<int, IntPtr> fieldHandles = new();       // token, _FieldHandle*
        internal readonly ConcurrentDictionary<int, IntPtr> methodHandles = new();      // token, _MethodHandle*

        // Private
        private _TypeHandle* primitivePtr = null;
        private Dictionary<int, ThreadContext> threadContexts = new Dictionary<int, ThreadContext>();
        private Dictionary<int, Module> loadedModules = new Dictionary<int, Module>();
        private Dictionary<int, Type> loadedTypes = new Dictionary<int, Type>();
        private Dictionary<int, Member> loadedMembers = new Dictionary<int, Member>();       


        // Constructor
        public AppContext()
        {
            // Allocate memory
            primitivePtr = (_TypeHandle*)NativeMemory.AllocZeroed((uint)sizeof(_TypeHandle) * (uint)RuntimeType.RuntimeTypeCodeSize + 1);

            // Init primitive types
            foreach(RuntimeTypeCode typeCode in Enum.GetValues<RuntimeTypeCode>())
            {
                primitivePtr[(int)typeCode] = new _TypeHandle(typeCode);
                typeHandles[(int)typeCode] = (IntPtr)(&primitivePtr[(int)typeCode]);
            }
        }

        ~AppContext()
        {
            NativeMemory.Free(primitivePtr);
        }

        // Methods
        public Module LoadModule(string modulePath, bool metadataOnly = false)
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

        public Module LoadModule(Stream stream, bool metadataOnly = false)
        {
            // Check for null
            if(stream == null)
                throw new ArgumentNullException(nameof(stream));

            // Read the module header
            ModuleName moduleName;
            Module.ReadModuleHeader(stream, out moduleName);

            // Check for already loaded
            Module loadedModule = ResolveModule(moduleName.Name, moduleName.Version);

            // Get loaded module
            if (loadedModule != null)
                return loadedModule;


            // Create the module and reader
            Module module = new Module(stream, moduleName);
            BinaryReader reader = new BinaryReader(stream);

            // Read module
            module.LoadMetadata(this, reader);

            // Check for executable
            if(metadataOnly == false)
                module.LoadExecutable(this, reader);

            // Register the module
            loadedModules[module.Token] = module;
            return module;
        }

        public Module ResolveModule(int token)
        {
            // Try to get the member
            Module module;
            if (loadedModules.TryGetValue(token, out module) == false)
                throw new DllNotFoundException("Token: " + token);

            return module;
        }

        public Module ResolveModule(string name)
        {
            return loadedModules.Values
                .FirstOrDefault(m => m.ModuleName.Name == name);
        }

        public Module ResolveModule(string name, Version version)
        {
            return loadedModules.Values
                .FirstOrDefault(m => m.ModuleName.Name == name 
                && m.ModuleName.Version.Equals(version));
        }

        public Member ResolveMember(int token)
        {
            // Try to get the member
            Member member;
            if (loadedMembers.TryGetValue(token, out member) == true)
                return member;

            // Check for type
            Type type;
            if (loadedTypes.TryGetValue(token, out type) == true)
                return type;

            // Not found
            throw new MissingMemberException("Token: " + token);
        }

        public T ResolveMember<T>(int token) where T : Member
        {
            return ResolveMember(token) as T;
        }

        public Type ResolveType(int token)
        {
            throw new NotImplementedException();
        }

        public Field ResolveField(int token)
        {
            // Try to get the member
            Member member;
            if (loadedMembers.TryGetValue(token, out member) == false || (member is Field) == false)
                throw new MissingMemberException("Token: " + token);

            return member as Field;
        }

        public Accessor ResolveAccessor(int token)
        {
            // Try to get the member
            Member member;
            if (loadedMembers.TryGetValue(token, out member) == false || (member is Accessor) == false)
                throw new MissingMemberException("Token: " + token);

            return member as Accessor;
        }

        public Method ResolveMethod(int token)
        {
            // Try to get the member
            Member member;
            if (loadedMembers.TryGetValue(token, out member) == false || (member is Method) == false)
                throw new MissingMemberException("Token: " + token);

            return member as Method;
        }

        public IEnumerable<Module> GetLibraries()
        {
            return loadedModules.Values;
        }

        public IEnumerable<Type> GetTypes(MemberFlags flags)
        {
            foreach(Type type in loadedTypes.Values)
            {
                // Check for field with flags
                if (type.HasMemberFlags(flags) == true)
                    yield return type;
            }
        }

        public void Dispose()
        {
        }

        internal void DefineMember(Member member)
        {
            // Check for already added
            if (loadedMembers.ContainsKey(member.Token) == true)
                throw new InvalidOperationException("Member with token already exists: " + member.Token);

            // Store member
            loadedMembers[member.Token] = member;
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
