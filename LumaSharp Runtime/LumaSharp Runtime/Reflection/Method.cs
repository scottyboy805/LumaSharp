
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime.Reflection
{
    public unsafe class Method : Member
    {
        // Type
        protected internal enum MethodFlags
        {
            Initializer = 1,
            Abstract = 2,
            Override = 4,
            Generic = 8,
        }

        // Private
        private MethodFlags methodFlags = 0;
        private Type returnType = null;
        private Parameter[] parameters = null;

        private _MethodHandle* methodExecutable = null;

        // Properties
        public bool IsInitializer
        {
            get { return (methodFlags & MethodFlags.Initializer) != 0; }
        }

        public bool IsAbstract
        {
            get { return (methodFlags & MethodFlags.Abstract) != 0; }
        }

        public bool IsOverride
        {
            get { return (methodFlags & MethodFlags.Override) != 0; }
        }

        public bool IsGeneric
        {
            get { return (methodFlags & MethodFlags.Generic) != 0; }
        }

        public Type ReturnType
        {
            get { return returnType; }
        }

        public Parameter[] Parameters
        {
            get { return parameters; }
        }

        public int ParameterCount
        {
            get { return parameters.Length; }
        }

        public IEnumerable<Type> ParameterTypes
        {
            get
            {
                foreach(Parameter parameter in parameters)
                {
                    yield return parameter.ParameterType;
                }
            }
        }

        // Constructor
        internal Method(string name, MethodFlags methodFlags, MemberFlags memberFlags) 
            : base(name, memberFlags)
        {
            this.methodFlags = methodFlags;
        }
    }
}
