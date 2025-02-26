using LumaSharp.Runtime.Handle;
using System.Diagnostics;

namespace LumaSharp.Runtime
{
    //internal unsafe struct CallSite
    //{
    //    // Public
    //    public CallSite* Parent;
    //    public _MethodHandle* Method;
    //    public byte* InstructionPtr;
    //    public byte* StackBasePtr;
    //    public byte* StackPtr;
    //    public byte* StackAllocPtr;

    //    // Constructor
    //    public CallSite(_MethodHandle* method, byte* stackBeginPtr, CallSite* parent = null)
    //    {
    //        this.Method = method;
    //        this.StackBasePtr = stackBeginPtr;
    //        this.StackPtr = stackBeginPtr + method->StackEvalOffset;
    //        this.StackAllocPtr = StackPtr + method->MaxStack;
    //        this.Parent = parent;
    //    }
    //}

    internal unsafe class ThreadContext
    {
        // Private
        private byte* instructionBasePtr = null;
        
        // Public
        public AppContext AppContext = null;
        public int ThreadID;                // Id of the executing thread
        public uint ThreadStackSize;         // Size of memory in bytes allocated for the evaluation stack
        public byte* ThreadStackPtr;        // Eval stack memory for this thread
        //public CallSite* CallSite;          // The call stack for the current execution

        // Constructor
        public ThreadContext(AppContext context, uint stackSize = 4096)
        {
            this.AppContext = context;
            this.ThreadID = Thread.CurrentThread.ManagedThreadId;
            this.ThreadStackSize = stackSize;
            this.ThreadStackPtr = __memory.InitStack(stackSize);
        }

        // Methods
        public void SetInstructionPtr(byte* instructionPtr)
        {
            this.instructionBasePtr = instructionPtr;
        }

        public void Throw<T>() where T : Exception, new()
        {
            throw new T();
        }

        [Conditional("DEBUG")]
        public void DebugInstruction(OpCode op, byte* pc)
        {
            Debug.WriteLine("{0:X4}: {1}", (IntPtr)(pc - instructionBasePtr), op);
        }

        [Conditional("DEBUG")]
        public void DebugInstruction(OpCode op, byte* pc, StackData* sp)
        {
            Debug.WriteLine("{0:X4}: {1}\t\t{2}", (IntPtr)(pc - instructionBasePtr), op, *sp);
        }

        [Conditional("DEBUG")]
        public void DebugInstruction(OpCode op, byte* pc, byte* mem, RuntimeTypeCode typeCode)
        {
            // Read value from memory
            StackData tmp;
            StackData.CopyFromMemory(&tmp, mem, typeCode);

            Debug.WriteLine("{0:X4}: {1}\t\t{2}", (IntPtr)(pc - instructionBasePtr), op, tmp);
        }

        [Conditional("DEBUG")]
        public void DebugInstruction(OpCode op, byte* pc, int branchOffset)
        {
            Debug.WriteLine("{0:X4}: {1}\t\t{2:X4}", (IntPtr)(pc - instructionBasePtr), op, (IntPtr)((pc + branchOffset) - instructionBasePtr));
        }

        [Conditional("DEBUG")]
        public void DebugInstruction(OpCode op, byte* pc, StackData* sp, int branchOffset)
        {
            Debug.WriteLine("{0:X4}: {1}\t\t{2} {3:X4}", (IntPtr)(pc - instructionBasePtr), op, *sp, (IntPtr)((pc + branchOffset) - instructionBasePtr));
        }
    }
}
