using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeCode = LumaSharp.Runtime.TypeCode;

namespace LumaSharp_RuntimeTests.Instructions
{
    [TestClass]
    public sealed class ExecuteInstructionsArithmetic
    {
        [TestMethod()]
        public void Arithmetic_Add_I4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Ld_I4, 2);
            builder.EmitOpCode(OpCode.Add, (byte)TypeCode.I32);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(7, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Add_I8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Ld_I8, 2L);
            builder.EmitOpCode(OpCode.Add, (byte)TypeCode.I64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(7L, __interpreter.FetchValue<long>());
            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Add_U8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Ld_I8, 2L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte) TypeCode.U8);
            builder.EmitOpCode(OpCode.Add, (byte)TypeCode.U64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(7UL, __interpreter.FetchValue<ulong>());
            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Add_Float()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.Ld_F4, 2.5f);
            builder.EmitOpCode(OpCode.Add, (byte)TypeCode.Float);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(8f, __interpreter.FetchValue<float>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Add_Double()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F8, 5.5d);
            builder.EmitOpCode(OpCode.Ld_F8, 2.5d);
            builder.EmitOpCode(OpCode.Add, (byte)TypeCode.Double);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(8d, __interpreter.FetchValue<double>());
            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
        }



        [TestMethod()]
        public void Arithmetic_Sub_I4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Ld_I4, 2);
            builder.EmitOpCode(OpCode.Sub, (byte)TypeCode.I32);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(3, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Sub_I8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Ld_I8, 2L);
            builder.EmitOpCode(OpCode.Sub, (byte)TypeCode.I64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(3L, __interpreter.FetchValue<long>());
            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Sub_U8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Ld_I8, 2L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Sub, (byte)TypeCode.U64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(3UL, __interpreter.FetchValue<ulong>());
            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Sub_Float()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.Ld_F4, 2.5f);
            builder.EmitOpCode(OpCode.Sub, (byte)TypeCode.Float);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(3f, __interpreter.FetchValue<float>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Sub_Double()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F8, 5.5d);
            builder.EmitOpCode(OpCode.Ld_F8, 2.5d);
            builder.EmitOpCode(OpCode.Sub, (byte)TypeCode.Double);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(3d, __interpreter.FetchValue<double>());
            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
        }



        [TestMethod()]
        public void Arithmetic_Mul_I4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Ld_I4, 2);
            builder.EmitOpCode(OpCode.Mul, (byte)TypeCode.I32);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(10, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Mul_I8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Ld_I8, 2L);
            builder.EmitOpCode(OpCode.Mul, (byte)TypeCode.I64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(10L, __interpreter.FetchValue<long>());
            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Mul_U8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Ld_I8, 2L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Mul, (byte)TypeCode.U64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(10UL, __interpreter.FetchValue<ulong>());
            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Mul_Float()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.Ld_F4, 2.5f);
            builder.EmitOpCode(OpCode.Mul, (byte)TypeCode.Float);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(13.75f, __interpreter.FetchValue<float>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Mul_Double()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F8, 5.5d);
            builder.EmitOpCode(OpCode.Ld_F8, 2.5d);
            builder.EmitOpCode(OpCode.Mul, (byte)TypeCode.Double);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(13.75d, __interpreter.FetchValue<double>());
            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
        }



        [TestMethod()]
        public void Arithmetic_Div_I4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 6);
            builder.EmitOpCode(OpCode.Ld_I4, 2);
            builder.EmitOpCode(OpCode.Div, (byte)TypeCode.I32);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(3, __interpreter.FetchValue<int>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Div_I8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 6L);
            builder.EmitOpCode(OpCode.Ld_I8, 2L);
            builder.EmitOpCode(OpCode.Div, (byte)TypeCode.I64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(3L, __interpreter.FetchValue<long>());
            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Div_U8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 6L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Ld_I8, 2L);
            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
            builder.EmitOpCode(OpCode.Div, (byte)TypeCode.U64);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(3UL, __interpreter.FetchValue<ulong>());
            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Div_Float()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F4, 11f);
            builder.EmitOpCode(OpCode.Ld_F4, 2f);
            builder.EmitOpCode(OpCode.Div, (byte)TypeCode.Float);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(5.5f, __interpreter.FetchValue<float>());
            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
        }

        [TestMethod()]
        public void Arithmetic_Div_Double()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F8, 11d);
            builder.EmitOpCode(OpCode.Ld_F8, 2d);
            builder.EmitOpCode(OpCode.Div, (byte)TypeCode.Double);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(5.5d, __interpreter.FetchValue<double>());
            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
        }
    }
}
