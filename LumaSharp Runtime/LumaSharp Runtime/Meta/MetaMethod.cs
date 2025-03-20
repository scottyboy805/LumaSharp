
using LumaSharp.Runtime.Handle;
using System.Diagnostics;

namespace LumaSharp.Runtime.Reflection
{
    [Flags]
    public enum MetaMethodFlags : ushort
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

    public unsafe class MetaMethod : MetaMember
    {
        // Private
        private readonly MetaMethodFlags methodFlags = 0;
        private readonly MemberReference<MetaType>[] returnTypeReferences = null;
        private readonly MetaVariable[] parameters = null;
        private readonly int rva = 0;

        // Properties
        public bool IsInitializer
        {
            get { return (methodFlags & MetaMethodFlags.Initializer) != 0; }
        }

        public bool IsAbstract
        {
            get { return (methodFlags & MetaMethodFlags.Abstract) != 0; }
        }

        public bool IsOverride
        {
            get { return (methodFlags & MetaMethodFlags.Override) != 0; }
        }

        public bool IsGeneric
        {
            get { return (methodFlags & MetaMethodFlags.Generic) != 0; }
        }

        public MetaType[] ReturnTypes
        {
            get { return returnTypeReferences.Select(r => r.Member).ToArray(); }
        }

        public MetaVariable[] Parameters
        {
            get { return parameters; }
        }

        public int ParameterCount
        {
            get { return parameters.Length; }
        }

        public IEnumerable<MetaType> ParameterTypes
        {
            get
            {
                foreach(MetaVariable parameter in parameters)
                {
                    yield return parameter.VariableType;
                }
            }
        }

        public int RVA
        {
            get { return rva; }
        }

        // Constructor
        internal MetaMethod(AppContext context)
            : base(context)
        { 
        }

        internal MetaMethod(AppContext context, _TokenHandle token, string name, MetaMethodFlags methodFlags, MemberReference<MetaType>[] returnTypes, MetaVariable[] parameters) 
            : base(context, token, name, (MemberFlags)methodFlags)
        {
            this.methodFlags = methodFlags;
            this.returnTypeReferences = returnTypes;
            this.parameters = parameters;
        }

        // Methods
        public object Invoke(object[] args, IntPtr instance = default)
        {
            // Get method handle
            _MethodHandle* method = (_MethodHandle*)context.methodHandles[Token];

            // Invoke the method
            StackData* stackPtr = InvokeHandle(method, args, instance);

            // Get return object
            object returnVal = null;

            // Check for return val
            if ((method->Signature.Flags & _MethodSignatureFlags.HasReturn) != 0)
            {
                // Unwrap return val
                StackData.Unwrap(stackPtr, out returnVal, method->Signature.ReturnParameters[0].TypeHandle.TypeCode);
            }

            return returnVal;

            //// Check for return value
            //if ((methodFlags & MethodFlags.ReturnValue) != 0)
            //{
            //    // Get type handle
            //    _TypeHandle returnTypeHandle = *ReturnType.typeExecutable;

            //    // Get return value
            //    return __memory.ReadAs(returnTypeHandle, (void*)stackPtr, -(int)returnTypeHandle.TypeSize);
            //}
            //return null;
        }

        public T Invoke<T>(object[] args, IntPtr instance = default) where T : unmanaged
        {
            // Get method handle
            _MethodHandle* method = (_MethodHandle*)context.methodHandles[Token];

            // Invoke the method
            StackData* stackPtr = InvokeHandle(method, args, instance);

            // Get return object
            T returnVal = default;

            // Check for return val
            if ((method->Signature.Flags & _MethodSignatureFlags.HasReturn) != 0)
            {
                // Check return type code


                // Unwrap return val
                StackData.UnwrapAs(stackPtr, &returnVal, method->Signature.ReturnParameters[0].TypeHandle.TypeCode);
            }

            return returnVal;
        }

        private StackData* InvokeHandle(_MethodHandle* method, object[] args, IntPtr instance)
        {
            // Check for no handle available
            if (method == null)
                throw new InvalidOperationException("Cannot invoke a method which has no handle");

            // Get thread context
            ThreadContext threadContext = context.GetCurrentThreadContext();

            // Alloc temp args
            StackData* spArg = stackalloc StackData[args.Length];

            // Init args
            for(int i = 0; i < args.Length; i++)
            {
                // Wrap arguments to stack object
                StackData.Wrap(spArg + i, args[i], method->Signature.Parameters[i].TypeHandle.TypeCode);
            }


            Stopwatch timer = Stopwatch.StartNew();

            // Invoke the method
            StackData* spReturn = _MethodHandle.Invoke(threadContext, method, instance, spArg);

            Console.WriteLine("Execution took: " + timer.Elapsed.TotalMilliseconds + "ms");

            // Get stack return address
            return spReturn;
        }

        internal void LoadMethodMetadata(BinaryReader reader)
        {
            //// Load member metadata
            //LoadMemberMetadata(reader);

            //// Get method flags
            //methodFlags = (MethodFlags)MemberFlags;


            //// Check for return type
            //if ((methodFlags & MethodFlags.ReturnValue) != 0)
            //{
            //    // Load return type
            //    this.returnTypeReference = new MemberReference<MetaType>(
            //        context, reader.ReadInt32());
            //}
            //else
            //{
            //    // Use void return type
            //    this.returnTypeReference = new MemberReference<MetaType>(
            //        context.ResolveType(0));
            //}

            //// Check for parameters
            //if ((methodFlags & MethodFlags.ParamValues) != 0)
            //{
            //    // Get parameter count
            //    int parameterCount = reader.ReadUInt16();

            //    // Initialize array
            //    this.parameters = new MetaVariable[parameterCount];

            //    // Load parameters
            //    for (int i = 0; i < parameterCount; i++)
            //    {
            //        // Create parameter
            //        MetaVariable parameter = new MetaVariable(context, i);

            //        // Read parameter
            //        parameter.LoadParameterMetadata(reader);

            //        // Register parameter
            //        parameters[i] = parameter;
            //    }
            //}
            //else
            //{
            //    // Use empty parameters
            //    this.parameters = new MetaVariable[0];
            //}
        }

        internal void LoadMethodExecutable(BinaryReader reader)
        {
            //// Read size of method
            //uint methodSize = reader.ReadUInt32();

            //// Create handle
            //methodExecutable = (_MethodHandle*)NativeMemory.AllocZeroed(methodSize);

            //// Read the handle
            //methodExecutable->Read(reader);


            //// Create arg locals handle
            //methodArgLocals = (_VariableHandle*)(methodExecutable + 1);
            //int argLocalCount = methodExecutable->Signature.ParameterCount + methodExecutable->Body.VariableCount;

            //// Read arg locals
            //for(int i = 0; i < argLocalCount; i++)
            //{
            //    // Read the arg local handle
            //    methodArgLocals[i].Read(reader);
            //}

            //// Create the instruction ptr
            //byte* instructionPtr = (byte*)(methodArgLocals + argLocalCount);
            //int instructionsSize = (int)(methodSize - (instructionPtr - (byte*)methodExecutable));

            //// Read bytecode instructions into existing allocated memory
            //reader.Read(new Span<byte>(instructionPtr, instructionsSize));

            // TODO
            //store method handle followed immediately by instructions

            // Method pointer starts at instructions but methodptr - sizeof(methodhandle) will access the method handle
        }
    }
}
