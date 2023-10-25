using LumaSharp.Runtime.Reflection;
using Type = LumaSharp.Runtime.Reflection.Type;

namespace LumaSharp.Runtime
{
    public sealed class AppContext : IDisposable
    {
        // Private
        private Dictionary<int, Library> loadedLibraries = new Dictionary<int, Library>();
        private Dictionary<int, Type> loadedTypes = new Dictionary<int, Type>();
        private Dictionary<int, Member> loadedMembers = new Dictionary<int, Member>();

        // Methods
        public void LoadLibrary(Stream stream)
        {

        }

        public Library GetLibrary(int token)
        {
            // Try to get the member
            Library library;
            if (loadedLibraries.TryGetValue(token, out library) == false)
                throw new DllNotFoundException("Token: " + token);

            return library;
        }

        public Library GetLibrary(string name)
        {
            return null;
        }

        public Library GetLibrary(string name, Version version)
        {
            return null;
        }

        public Member GetMember(int token)
        {
            // Try to get the member
            Member member;
            if (loadedMembers.TryGetValue(token, out member) == false)
                throw new MissingMemberException("Token: " + token);

            return member;
        }

        public Field GetField(int token)
        {
            // Try to get the member
            Member member;
            if (loadedMembers.TryGetValue(token, out member) == false || (member is Field) == false)
                throw new MissingMemberException("Token: " + token);

            return member as Field;
        }

        public Accessor GetAccessor(int token)
        {
            // Try to get the member
            Member member;
            if (loadedMembers.TryGetValue(token, out member) == false || (member is Accessor) == false)
                throw new MissingMemberException("Token: " + token);

            return member as Accessor;
        }

        public Method GetMethod(int token)
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


    }
}
