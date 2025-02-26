
namespace LumaSharp.Runtime.Handle
{
    [Flags]
    public enum _MethodSignatureFlags : uint
    {
        HasArguments = 1 << 0,
        HasReturn = 1 << 1,
        VoidCall = 1 << 2,
    }

    public unsafe readonly struct _MethodSignature
    {
        // Public
        public readonly _MethodSignatureFlags Flags;        
        public readonly _VariableHandle* Parameters;
        public readonly _VariableHandle* ReturnParameter;
        public readonly ushort ParameterCount;

        // Constructor
        public _MethodSignature(ushort parameterCount, _VariableHandle* parameters, _VariableHandle* returnParameter)
        {
            this.Flags = 0;
            this.Parameters = parameters;
            this.ReturnParameter = returnParameter;
            this.ParameterCount = parameterCount;
        }
    }

    public unsafe readonly struct _MethodBodyHandle
    {
        // Public
        public readonly ushort MaxStack;        
        public readonly _VariableHandle* Variables; 
        public readonly ushort VariableCount;

        // Constructor
        public _MethodBodyHandle(ushort maxStack, _VariableHandle* variables, ushort variableCount)
        {
            this.MaxStack = maxStack;
            this.Variables = variables;
            this.VariableCount = variableCount;
        }
    }

    public unsafe readonly struct _MethodHandle
    {
        // Internal
        public readonly int MethodToken;
        public readonly _MethodSignature Signature;
        public readonly _MethodBodyHandle Body;        

        // Constructor
        public _MethodHandle(int token, _MethodSignature signature, _MethodBodyHandle body)
        {
            this.MethodToken = token;
            this.Signature = signature;
            this.Body = body;
        }

        // Methods       
        internal void Write(BinaryWriter writer)
        {
            //writer.Write(MethodToken);
            //writer.Write(MaxStack);
            //writer.Write(ArgCount);
            //writer.Write(LocalCount);
        }

        internal void Read(BinaryReader reader)
        {
            //MethodToken = reader.ReadInt32();
            //MaxStack = reader.ReadUInt16();
            //ArgCount = reader.ReadUInt16();
            //LocalCount = reader.ReadUInt16();
        }
    }
}
