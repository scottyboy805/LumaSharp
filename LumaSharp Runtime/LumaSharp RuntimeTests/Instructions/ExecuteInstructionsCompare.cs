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



        [TestMethod()]
        public void Compare_Less_I4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 2);
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Cmp_L, (byte)TypeCode.I32);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_Less_I8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 2L);
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cmp_L, (byte)TypeCode.I64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_Less_U8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 2L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Cmp_L, (byte)TypeCode.U64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_Less_Float()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F4, 2.5f);
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.Cmp_L, (byte)TypeCode.Float);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_Less_Double()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F8, 2.5d);
            builder.EmitOpCode(OpCode.Ld_F8, 5.5);
            builder.EmitOpCode(OpCode.Cmp_L, (byte)TypeCode.Double);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }



        [TestMethod()]
        public void Compare_LessEqual_I4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 2);
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Cmp_Le, (byte)TypeCode.I32);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_LessEqual_I8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 2L);
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cmp_Le, (byte)TypeCode.I64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_LessEqual_U8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 2L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Cmp_Le, (byte)TypeCode.U64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_LessEqual_Float()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F4, 2.5f);
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.Cmp_Le, (byte)TypeCode.Float);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_LessEqual_Double()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F8, 2.5d);
            builder.EmitOpCode(OpCode.Ld_F8, 5.5);
            builder.EmitOpCode(OpCode.Cmp_Le, (byte)TypeCode.Double);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }



        [TestMethod()]
        public void Compare_Greater_I4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 7);
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Cmp_G, (byte)TypeCode.I32);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_Greater_I8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 7L);
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cmp_G, (byte)TypeCode.I64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_Greater_U8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 7L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Cmp_G, (byte)TypeCode.U64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_Greater_Float()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F4, 7.5f);
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.Cmp_G, (byte)TypeCode.Float);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_Greater_Double()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F8, 7.5d);
            builder.EmitOpCode(OpCode.Ld_F8, 5.5);
            builder.EmitOpCode(OpCode.Cmp_G, (byte)TypeCode.Double);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }



        [TestMethod()]
        public void Compare_GreaterEqual_I4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 7);
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Cmp_Ge, (byte)TypeCode.I32);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_GreaterEqual_I8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 7L);
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cmp_Ge, (byte)TypeCode.I64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_GreaterEqual_U8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 7L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Cmp_Ge, (byte)TypeCode.U64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_GreaterEqual_Float()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F4, 7.5f);
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.Cmp_Ge, (byte)TypeCode.Float);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Compare_GreaterEqual_Double()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F8, 7.5d);
            builder.EmitOpCode(OpCode.Ld_F8, 5.5);
            builder.EmitOpCode(OpCode.Cmp_Ge, (byte)TypeCode.Double);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(1, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }
    }
}
