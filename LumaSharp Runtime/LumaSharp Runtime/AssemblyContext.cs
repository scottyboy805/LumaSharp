using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime.Reflection;
using System.Collections.Concurrent;

namespace LumaSharp.Runtime
{
    public unsafe sealed class AssemblyContext
    {
        // Internal
        internal readonly ConcurrentDictionary<_TokenHandle, IntPtr> typeHandles = new();        // token, _TypeHandle*
        internal readonly ConcurrentDictionary<_TokenHandle, IntPtr> fieldHandles = new();       // token, _FieldHandle*
        internal readonly ConcurrentDictionary<_TokenHandle, IntPtr> methodHandles = new();      // token, _MethodHandle*
        internal readonly ConcurrentDictionary<_TokenHandle, IntPtr> globalMemoryHandles = new();

        internal readonly List<string> metaStrings = new();
        internal readonly Dictionary<_TokenHandle, MetaMember> metaMembers = new();

        // Private
        private readonly AppContext appContext;
        private readonly MetaAssembly assembly;
        private readonly MetaAssemblyName assemblyName;
        private readonly List<AssemblyContext> referenceAssemblies;
        private readonly List<AssemblyContext> referencedByAssemblies;
        //private readonly Dictionary<_TokenHandle, MetaType> loadedTypes = new();
        //private readonly Dictionary<_TokenHandle, MetaMember> loadedMembers = new();

        // Properties
        public AppContext AppContext => appContext;
        public MetaAssembly Assembly => assembly;
        public MetaAssemblyName AssemblyName => assemblyName;
        public IReadOnlyList<AssemblyContext> ReferenceAssemblies => referenceAssemblies != null ? referenceAssemblies : Array.Empty<AssemblyContext>();
        public bool HasReferenceAssemblies => referenceAssemblies != null && referencedByAssemblies.Count > 0;
        public bool IsReferencedByAssemblies => referencedByAssemblies != null && referencedByAssemblies.Count > 0;

        // Constructor
        internal AssemblyContext(AppContext appContext) 
        {
            this.appContext = appContext;

            // Ensure primitive types are registered for referencing
            InitializePrimitiveTypes();
        }

        internal AssemblyContext(AppContext appContext, MetaAssembly assembly)
        {
            this.appContext = appContext;
            this.assembly = assembly;
            this.assemblyName = assembly.AssemblyName;

            // Ensure primitive types are registered for referencing
            InitializePrimitiveTypes();
        }

        // Methods
        private void InitializePrimitiveTypes()
        {
            // Init primitive types
            foreach (RuntimeTypeCode typeCode in Enum.GetValues<RuntimeTypeCode>())
            {
                typeHandles[typeCode] = (IntPtr)(&appContext.primitivePtr[(int)typeCode]);
            }
        }

        public IEnumerable<MetaType> GetTypes(MetaMemberFlags flags)
        {
            foreach (MetaType type in metaMembers.Values)
            {
                // Check for field with flags
                if (type.HasMemberFlags(flags) == true)
                    yield return type;
            }
        }

        public string ResolveString(_TokenHandle token)
        {
            // Check for invalid or not string
            if (token.IsNil == true || token.Kind != TokenKind.StringReference)
                return null;

            // Get the indexed string
            return metaStrings[token.Row];
        }

        public MetaMember ResolveMember(_TokenHandle token)
        {
            // Try to get the member
            MetaMember member;
            if (metaMembers.TryGetValue(token, out member) == true)
                return member;

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
            // Check for nil
            if(token.IsNil == true)
                return null;

            // Try to get the member
            MetaMember member;
            if (metaMembers.TryGetValue(token, out member) == false || (member is MetaField) == false)
                throw new MissingMemberException("Token: " + token);

            return member as MetaField;
        }

        public MetaAccessor ResolveAccessor(_TokenHandle token)
        {
            // Try to get the member
            MetaMember member;
            if (metaMembers.TryGetValue(token, out member) == false || (member is MetaAccessor) == false)
                throw new MissingMemberException("Token: " + token);

            return member as MetaAccessor;
        }

        public MetaMethod ResolveMethod(_TokenHandle token)
        {
            // Try to get the member
            MetaMember member;
            if (metaMembers.TryGetValue(token, out member) == false || (member is MetaMethod) == false)
                throw new MissingMemberException("Token: " + token);

            return member as MetaMethod;
        }

        internal void DefineMetaMember(MetaMember member)
        {
            // Check for already added
            if (metaMembers.ContainsKey(member.Token) == true)
                throw new InvalidOperationException("Member with token already exists: " + member.Token);

            // Store member
            metaMembers[member.Token] = member;
        }

        internal void DefineExecutableMethod(_MethodHandle* method)
        {
            // Check for already added
            if (methodHandles.ContainsKey(method->MethodToken) == true)
                throw new InvalidOperationException("Method executable with token already exists: " + method->MethodToken);

            // Store method
            methodHandles[method->MethodToken] = (IntPtr)method;
        }
    }
}
