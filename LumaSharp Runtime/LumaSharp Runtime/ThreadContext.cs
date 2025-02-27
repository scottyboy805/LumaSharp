using LumaSharp.Runtime.Handle;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LumaSharp.Runtime
{
    internal unsafe class ThreadContext
    {
        // Type
        private readonly struct CallSite
        {
            // Public
            public readonly _MethodHandle* Method;
            public readonly byte* InstructionPtr;
            public readonly StackData* StackVarPtr;
            public readonly StackData* StackPtr;

            // Constructor
            public CallSite(_MethodHandle* method, byte* instructionPtr, StackData* stackVarPtr, StackData* stackPtr)
            {
                this.Method = method;
                this.InstructionPtr = instructionPtr;
                this.StackVarPtr = stackVarPtr;
                this.StackPtr = stackPtr;
            }
        }

        // Private
        private byte* instructionBasePtr = null;
        private int callDepth = 0;
        private readonly Stack<CallSite> callStack = new Stack<CallSite>();  // The call stack for the current execution

        // Public
        public readonly AppContext AppContext = null;
        public readonly int ThreadID;                                       // Id of the executing thread
        public readonly uint ThreadStackSize;                               // Size of memory in bytes allocated for the evaluation stack
        public readonly byte* ThreadStackPtr;                               // Eval stack memory for this thread        

        // Properties
        public int CallDepth => callDepth;

        // Constructor
        public ThreadContext(AppContext context, uint stackSize = 4096)
        {
            this.AppContext = context;
            this.ThreadID = Thread.CurrentThread.ManagedThreadId;
            this.ThreadStackSize = stackSize;
            this.ThreadStackPtr = __memory.InitStack(stackSize);
        }

        // Methods      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushCall(_MethodHandle* method, byte* instructionPtr, StackData* stackVarPtr, StackData* stackPtr)
        {
            // Push call stack
            callStack.Push(new CallSite(method, instructionPtr, stackVarPtr, stackPtr));
            callDepth++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopCall(out _MethodHandle* method, out byte* instructionPtr, out StackData* stackVarPtr, out StackData* stackPtr)
        {
            // Pop call stack
            CallSite call = callStack.Pop();

            // Get call site
            method = call.Method;
            instructionPtr = call.InstructionPtr;
            stackVarPtr = call.StackVarPtr;
            stackPtr = call.StackPtr;

            // Decrement call depth
            callDepth--;
        }

        public void Throw<T>() where T : Exception, new()
        {
            throw new T();
        }

        [Conditional("DEBUG")]
        public void DebugInstructionPtr(byte* instructionPtr)
        {
            this.instructionBasePtr = instructionPtr;
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
