using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeCode = LumaSharp.Runtime.TypeCode;

namespace LumaSharp_RuntimeTests.Instructions
{
    [TestClass]
    public unsafe sealed class ExecuteInstructionsArray
    {
        [TestMethod()]
        public void Array_Create_I32()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.NewArr, (int)TypeCode.I32);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    maxStack = 8,
                };

                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value (8 byte offset for maxstack, p[lus 12 byte offset for memory handle
            Assert.AreEqual(__memory.stackBasePtr + 20, __interpreter.FetchValue<IntPtr>());
        }

        [TestMethod()]
        public void Array_Store_I32()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 123); // Value to store
            builder.EmitOpCode(OpCode.Ld_I4, 0); // Array index
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.NewArr, (int)TypeCode.I32);
            builder.EmitOpCode(OpCode.St_Elem);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    maxStack = 8,
                };

                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value (8 byte offset for maxstack, p[lus 12 byte offset for memory handle
            Assert.AreEqual(123, __interpreter.FetchValue<int>(20));
        }
    }
}
