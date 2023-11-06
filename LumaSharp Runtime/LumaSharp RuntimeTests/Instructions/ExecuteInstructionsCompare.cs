using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeCode = LumaSharp.Runtime.TypeCode;

namespace LumaSharp_RuntimeTests.Instructions
{
    [TestClass]
    public sealed class ExecuteInstructionsCompare
    {
        [TestMethod()]
        public void Compare_Equal_I4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Cmp_Eq, (byte)TypeCode.I32);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_Equal_I8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cmp_Eq, (byte)TypeCode.I64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_Equal_U8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Cmp_Eq, (byte)TypeCode.U64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_Equal_Float()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.Cmp_Eq, (byte)TypeCode.Float);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_Equal_Double()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F8, 5.5d);
            builder.EmitOpCode(OpCode.Ld_F8, 5.5);
            builder.EmitOpCode(OpCode.Cmp_Eq, (byte)TypeCode.Double);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }



        [TestMethod()]
        public void Compare_NotEqual_I4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Cmp_NEq, (byte)TypeCode.I32);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(0, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_NotEqual_I8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cmp_NEq, (byte)TypeCode.I64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(0, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_NotEqual_U8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Cmp_NEq, (byte)TypeCode.U64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(0, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_NotEqual_Float()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.Cmp_NEq, (byte)TypeCode.Float);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(0, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_NotEqual_Double()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F8, 5.5d);
            builder.EmitOpCode(OpCode.Ld_F8, 5.5);
            builder.EmitOpCode(OpCode.Cmp_NEq, (byte)TypeCode.Double);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(0, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }
    }
}
