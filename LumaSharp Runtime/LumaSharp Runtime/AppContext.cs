using LumaSharp.Runtime.Reflection;
using Type = LumaSharp.Runtime.Reflection.Type;

namespace LumaSharp.Runtime
{
    public sealed class AppContext : IDisposable
    {
        // Private
        private Dictionary<int, ThreadContext> threadContexts = new Dictionary<int, ThreadContext>();
        private Dictionary<int, Library> loadedLibraries = new Dictionary<int, Library>();
        private Dictionary<int, Type> loadedTypes = new Dictionary<int, Type>();
        private Dictionary<int, Member> loadedMembers = new Dictionary<int, Member>();

        // Methods
        public void LoadLibrary(Stream stream)
        {

        }

        public Library ResolveLibrary(int token)
        {
            // Try to get the member
            Library library;
            if (loadedLibraries.TryGetValue(token, out library) == false)
                throw new DllNotFoundException("Token: " + token);

            return library;
        }

        public Library ResolveLibrary(string name)
        {
            return null;
        }

        public Library ResolveLibrary(string name, Version version)
        {
            return null;
        }

        public Member ResolveMember(int token)
        {
            // Try to get the member
            Member member;
            if (loadedMembers.TryGetValue(token, out member) == false)
                throw new MissingMemberException("Token: " + token);

            return member;
        }

        public T ResolveMember<T>(int token) where T : Member
        {
            return ResolveMember<T>(token) as T;
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

        public IEnumerable<Library> GetLibraries()
        {
            return loadedLibraries.Values;
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
