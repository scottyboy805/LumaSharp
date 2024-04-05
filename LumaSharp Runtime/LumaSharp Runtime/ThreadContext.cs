using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime
{
    internal unsafe struct CallSite
    {
        // Public
        public CallSite* Parent;
        public _MethodHandle* Method;
        public byte* InstructionPtr;
        public byte* StackBasePtr;
        public byte* StackPtr;
        public byte* StackAllocPtr;

        // Constructor
        public CallSite(_MethodHandle* method, byte* stackBeginPtr, CallSite* parent = null)
        {
            this.Method = method;
            this.StackBasePtr = stackBeginPtr;
            this.StackPtr = stackBeginPtr + method->StackPtrOffset;
            this.StackAllocPtr = StackPtr + method->MaxStack;
            this.Parent = parent;
        }
    }

    internal unsafe class ThreadContext
    {
        // Public
        public int ThreadID;                // Id of the executing thread
        public uint ThreadStackSize;         // Size of memory in bytes allocated for the evaluation stack
        public byte* ThreadStackPtr;        // Eval stack memory for this thread
        public CallSite* CallSite;          // The call stack for the current execution
    }
}
