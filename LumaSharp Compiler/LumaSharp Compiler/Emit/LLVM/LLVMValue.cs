using LLVMSharp.Interop;

namespace LumaSharp.Compiler.Emit.LLVM
{
    internal readonly struct LLVMValue
    {
        // Public
        public readonly LLVMTypeRef Type;
        public readonly LLVMValueRef Value;
        public readonly bool IsPointer;

        // Constructor
        public LLVMValue(LLVMTypeRef type, LLVMValueRef value, bool isPointer = false)
        {
            this.Type = type;
            this.Value = value;
            this.IsPointer = isPointer;
        }
    }
}
