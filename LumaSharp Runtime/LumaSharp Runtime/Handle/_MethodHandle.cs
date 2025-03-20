using System.Reflection;

namespace LumaSharp.Runtime.Handle
{
    [Flags]
    public enum _MethodSignatureFlags : ushort
    {
        HasThis = 1 << 0,
        HasArguments = 1 << 1,
        HasReturn = 1 << 2,
        VoidCall = 1 << 3,
    }

    public unsafe readonly struct _MethodSignatureHandle
    {
        // Public
        public readonly _MethodSignatureFlags Flags;        
        public readonly _VariableHandle* Parameters;
        public readonly _VariableHandle* ReturnParameters;
        public readonly ushort ParameterCount;
        public readonly ushort ReturnCount;

        // Constructor
        public _MethodSignatureHandle(ushort parameterCount, ushort returnCount, _MethodSignatureFlags flags)
        {
            this.ParameterCount = parameterCount;
            this.Flags = flags;
            this.Parameters = null;
            this.ReturnCount = returnCount;
            this.ReturnParameters = null;
        }

        public _MethodSignatureHandle(ushort parameterCount, ushort returnCount, _VariableHandle* parameters, _VariableHandle* returnParameter, _MethodSignatureFlags flags)
        {
            this.Flags = flags;
            this.Parameters = parameters;
            this.ReturnParameters = returnParameter;
            this.ParameterCount = parameterCount;
            this.ReturnCount = returnCount;
        }
    }

    public unsafe readonly struct _MethodBodyHandle
    {
        // Public
        public readonly ushort MaxStack;        
        public readonly _VariableHandle* Variables; 
        public readonly ushort VariableCount;
        public readonly uint InstructionsSize;

        // Constructor
        public _MethodBodyHandle(ushort maxStack, ushort variableCount, uint instructionsSize)
        {
            this.MaxStack = maxStack;
            this.Variables = null;
            this.VariableCount = variableCount;
            this.InstructionsSize = instructionsSize;
        }

        public _MethodBodyHandle(ushort maxStack, _VariableHandle* variables, ushort variableCount, uint instructionsSize)
        {
            this.MaxStack = maxStack;
            this.Variables = variables;
            this.VariableCount = variableCount;
            this.InstructionsSize = instructionsSize;
        }
    }

    public unsafe readonly struct _MethodHandle
    {
        // Internal
        public readonly _TokenHandle MethodToken;
        public readonly _TokenHandle DeclaringTypeToken;
        public readonly _MethodSignatureHandle Signature;
        public readonly _MethodBodyHandle Body;

        // Constructor
        public _MethodHandle(_TokenHandle methodToken, _TokenHandle declaringTypeToken, _MethodSignatureHandle signature, _MethodBodyHandle body)
        {
            this.MethodToken = methodToken;
            this.DeclaringTypeToken = declaringTypeToken;
            this.Signature = signature;
            this.Body = body;
        }

        // Methods       
        internal static StackData* Invoke(ThreadContext context, _MethodHandle* method, IntPtr inst, StackData* args)
        {
            // Get spArg address
            StackData* spArg = (StackData*)context.ThreadStackPtr;

            // Check for this
            if ((method->Signature.Flags & _MethodSignatureFlags.HasThis) != 0)
            { 
                // Check for null instance
                if(inst == IntPtr.Zero)
                    throw new TargetInvocationException("Non-static method requires an instance", null);

                // Load instance
                spArg->Type = StackTypeCode.Address;
                spArg->Ptr = inst;
                spArg++;
            }

            // Load args
            for(int i = 0; i < method->Signature.ParameterCount; i++)
            {
                // Copy arg value
                StackData.CopyStack(&args[i], spArg);
                spArg++;
            }
            
            // Execute bytecode
            return __interpreter.ExecuteBytecode(context, method);
        }
    }
}
