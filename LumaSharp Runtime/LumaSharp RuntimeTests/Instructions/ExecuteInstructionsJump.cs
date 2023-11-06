using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_RuntimeTests.Instructions
{
    [TestClass]
    public sealed class ExecuteInstructionsJump
    {
        [TestMethod()]
        public void Jump()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            int offset = 9; // 4 byte jmp offset, 1 byte ld instruction, 4 byte ld data

            // Emit instruction
            builder.EmitOpCode(OpCode.Jmp, offset);
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(__memory.stackBasePtr, stackPtr);
        }

        [TestMethod()]
        public void Jump_0()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            int offset = 9; // 4 byte jmp offset, 1 byte ld instruction, 4 byte ld data

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4_0);
            builder.EmitOpCode(OpCode.Jmp_0, offset);
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(__memory.stackBasePtr, stackPtr);
        }

        [TestMethod()]
        public void Jump_1()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            int offset = 9; // 4 byte jmp offset, 1 byte ld instruction, 4 byte ld data

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4_1);
            builder.EmitOpCode(OpCode.Jmp_1, offset);
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(__memory.stackBasePtr, stackPtr);
        }
    }
}
