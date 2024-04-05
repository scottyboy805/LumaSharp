using LumaSharp.Runtime.Reflection;
using System.Runtime.InteropServices;
using Type = LumaSharp.Runtime.Reflection.Type;

namespace LumaSharp.Runtime
{
    public unsafe sealed class AppContext : IDisposable
    {
        // Internal
        internal _TypeHandle* _anyType = null;

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
            primitivePtr = (_TypeHandle*)NativeMemory.AllocZeroed((uint)sizeof(_TypeHandle) * 13);
            _TypeHandle* _any = _anyType = primitivePtr;
            _TypeHandle* _bool = primitivePtr + 1;
            _TypeHandle* _char = primitivePtr + 2;
            _TypeHandle* _i8 = primitivePtr + 3;
            _TypeHandle* _u8 = primitivePtr + 4;
            _TypeHandle* _i16 = primitivePtr + 5;
            _TypeHandle* _u16 = primitivePtr + 6;
            _TypeHandle* _i32 = primitivePtr + 7;
            _TypeHandle* _u32 = primitivePtr + 8;
            _TypeHandle* _i64 = primitivePtr + 9;
            _TypeHandle* _u64 = primitivePtr + 10;
            _TypeHandle* _f32 = primitivePtr + 11;
            _TypeHandle* _f64 = primitivePtr + 12;


            // Create handles
            *_any = new _TypeHandle
            {
                TypeToken = (int)TypeCode.Any,
                TypeSize = (uint)sizeof(IntPtr),
            };
            *_bool = new _TypeHandle
            {
                TypeToken = (int)TypeCode.Bool,
                TypeSize = sizeof(bool),
            };
            *_char = new _TypeHandle
            {
                TypeToken = (int)TypeCode.Char,
                TypeSize = sizeof(char),
            };
            *_i8 = new _TypeHandle
            {
                TypeToken = (int)TypeCode.I8,
                TypeSize = sizeof(sbyte),
            };
            *_u8 = new _TypeHandle
            {
                TypeToken = (int)TypeCode.I8,
                TypeSize = sizeof(byte),
            };
            *_i16 = new _TypeHandle
            {
                TypeToken = (int)TypeCode.I16,
                TypeSize = sizeof(short),
            };
            *_u16 = new _TypeHandle
            {
                TypeToken = (int)TypeCode.U16,
                TypeSize = sizeof(ushort),
            };
            *_i32 = new _TypeHandle
            {
                TypeToken = (int)TypeCode.I32,
                TypeSize = sizeof(int),
            };
            *_u32 = new _TypeHandle
            {
                TypeToken = (int)TypeCode.U32,
                TypeSize = sizeof(uint),
            };
            *_i64 = new _TypeHandle
            {
                TypeToken = (int)TypeCode.I64,
                TypeSize = sizeof(long),
            };
            *_u64 = new _TypeHandle
            {
                TypeToken = (int)TypeCode.U64,
                TypeSize = sizeof(ulong),
            };
            *_f32 = new _TypeHandle
            {
                TypeToken = (int)TypeCode.F32,
                TypeSize = sizeof(float),
            };
            *_f64 = new _TypeHandle
            {
                TypeToken = (int)TypeCode.F64,
                TypeSize = sizeof(double),
            };

            // Register primitives
            loadedTypes[(int)TypeCode.Any] = new PrimitiveType(this, TypeCode.Any, _any);
            loadedTypes[(int)TypeCode.Bool] = new PrimitiveType(this, TypeCode.Bool, _bool);
            loadedTypes[(int)TypeCode.Char] = new PrimitiveType(this, TypeCode.Char, _char);
            loadedTypes[(int)TypeCode.I8] = new PrimitiveType(this, TypeCode.I8, _i8);
            loadedTypes[(int)TypeCode.U8] = new PrimitiveType(this, TypeCode.U8, _u8);
            loadedTypes[(int)TypeCode.I16] = new PrimitiveType(this, TypeCode.I16, _i16);
            loadedTypes[(int)TypeCode.U16] = new PrimitiveType(this, TypeCode.U16, _u16);
            loadedTypes[(int)TypeCode.I32] = new PrimitiveType(this, TypeCode.I32, _i32);
            loadedTypes[(int)TypeCode.U32] = new PrimitiveType(this, TypeCode.U32, _u32);
            loadedTypes[(int)TypeCode.I64] = new PrimitiveType(this, TypeCode.I64, _i64);
            loadedTypes[(int)TypeCode.U64] = new PrimitiveType(this, TypeCode.U64, _u64);
            loadedTypes[(int)TypeCode.F32] = new PrimitiveType(this, TypeCode.F32, _f32);
            loadedTypes[(int)TypeCode.F64] = new PrimitiveType(this, TypeCode.F64, _f64);
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
            context = new ThreadContext();

            // Initialize context
            context.ThreadID = Thread.CurrentThread.ManagedThreadId;
            context.ThreadStackSize = stackSize;
            context.ThreadStackPtr = __memory.InitStack(stackSize);
            context.CallSite = null;

            // Register context
            threadContexts[threadID] = context;

            // Get new context
            return context;
        }
    }
}
