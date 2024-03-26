
using LumaSharp.Runtime.Handle;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LumaSharp.Runtime.Reflection
{
    [Flags]
    public enum MethodFlags : uint
    {
        Export = 1,
        Internal = 2,
        Hidden = 4,
        Global = 8,
        ReturnValue = 16,
        ParamValues = 32,
        Initializer = 64,
        Abstract = 128,
        Override = 256,
        Generic = 512,
    }

    public unsafe class Method : Member
    {
        // Private
        private MethodFlags methodFlags = 0;
        private MemberReference<Type> returnTypeReference = null;
        private Parameter[] parameters = null;

        // Internal
        internal _MethodHandle* methodExecutable = null;
        internal _StackHandle* methodArgLocals = null;

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
            get { return returnTypeReference.Member; }
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
        internal Method(AppContext context)
            : base(context)
        { 
        }

        internal Method(AppContext context, string name, MethodFlags methodFlags) 
            : base(context, name, (MemberFlags)methodFlags)
        {
            this.methodFlags = methodFlags;
        }

        // Methods
        public object Invoke(object[] args, IntPtr instance = default)
        {
            // Invoke the method
            nint stackPtr = InvokeMethodHandle(instance, args);

            // Check for return value
            if ((methodFlags & MethodFlags.ReturnValue) != 0)
            {
                // Get type handle
                _TypeHandle returnTypeHandle = *ReturnType.typeExecutable;

                // Get return value
                return __memory.ReadAs(returnTypeHandle, (void*)stackPtr, -(int)returnTypeHandle.TypeSize);
            }
            return null;
        }

        public T Invoke<T>(object[] args, IntPtr instance = default) where T : unmanaged
        {
            // Invoke the method
            nint stackPtr = InvokeMethodHandle(instance, args);

            // Check for return value
            if((methodFlags & MethodFlags.ReturnValue) != 0)
            {
                // Get return value
                return __memory.ReadAs<T>((void*)stackPtr, -sizeof(T));
            }
            return default;
        }

        private nint InvokeMethodHandle(IntPtr instance, object[] args)
        {
            // Check for no handle available
            if (methodExecutable == null)
                throw new InvalidOperationException("Cannot invoke a method which has no handle");

            // Get thread context
            ThreadContext threadContext = context.GetCurrentThreadContext();


            // Load instance
            if(IsGlobal == false)
            {
                // Push instance
                //__memory.WriteAs()
            }

            // Load arguments
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    // Push argument
                    __memory.WriteAs(args[i], parameters[i].ParameterType.typeExecutable, ref threadContext.ThreadStackPtr);
                }
            }


            Stopwatch timer = Stopwatch.StartNew();

            // Invoke the method
            nint result = (nint)__interpreter.ExecuteBytecode(context, threadContext, methodExecutable);

            Console.WriteLine("Execution took: " + timer.Elapsed.TotalMilliseconds + "ms");
            return result;
        }

        internal void LoadMethodMetadata(BinaryReader reader)
        {
            // Load member metadata
            LoadMemberMetadata(reader);

            // Get method flags
            methodFlags = (MethodFlags)MemberFlags;


            // Check for return type
            if ((methodFlags & MethodFlags.ReturnValue) != 0)
            {
                // Load return type
                this.returnTypeReference = new MemberReference<Type>(
                    context, reader.ReadInt32());
            }
            else
            {
                // Use void return type
                this.returnTypeReference = new MemberReference<Type>(
                    context.ResolveType(0));
            }

            // Check for parameters
            if ((methodFlags & MethodFlags.ParamValues) != 0)
            {
                // Get parameter count
                int parameterCount = reader.ReadUInt16();

                // Initialize array
                this.parameters = new Parameter[parameterCount];

                // Load parameters
                for (int i = 0; i < parameterCount; i++)
                {
                    // Create parameter
                    Parameter parameter = new Parameter(context, i);

                    // Read parameter
                    parameter.LoadParameterMetadata(reader);

                    // Register parameter
                    parameters[i] = parameter;
                }
            }
            else
            {
                // Use empty parameters
                this.parameters = new Parameter[0];
            }
        }

        internal void LoadMethodExecutable(BinaryReader reader)
        {
            // Read size of method
            uint methodSize = reader.ReadUInt32();

            // Create handle
            methodExecutable = (_MethodHandle*)NativeMemory.AllocZeroed(methodSize);

            // Read the handle
            methodExecutable->Read(reader);


            // Create arg locals handle
            methodArgLocals = (_StackHandle*)(methodExecutable + 1);
            int argLocalCount = methodExecutable->ArgCount + methodExecutable->LocalCount;

            // Read arg locals
            for(int i = 0; i < argLocalCount; i++)
            {
                // Read the arg local handle
                methodArgLocals[i].Read(reader);
            }

            // Create the instruction ptr
            byte* instructionPtr = (byte*)(methodArgLocals + argLocalCount);
            int instructionsSize = (int)(methodSize - (instructionPtr - (byte*)methodExecutable));

            // Read bytecode instructions into existing allocated memory
            reader.Read(new Span<byte>(instructionPtr, instructionsSize));

            // TODO
            //store method handle followed immediately by instructions

            // Method pointer starts at instructions but methodptr - sizeof(methodhandle) will access the method handle
        }
    }
}
