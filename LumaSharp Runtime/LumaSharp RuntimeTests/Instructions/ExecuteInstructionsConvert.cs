//using LumaSharp.Runtime.Emit;
//using LumaSharp.Runtime;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using TypeCode = LumaSharp.Runtime.TypeCode;

//namespace LumaSharp_RuntimeTests.Instructions
//{
//    [TestClass]
//    public sealed class ExecuteInstructionsConvert
//    {
//        //[TestMethod()]
//        //public void Convert_I1_U4()
//        //{
//        //    // Create builder
//        //    MemoryStream stream = new MemoryStream();
//        //    InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//        //    // Emit instruction
//        //    builder.EmitOpCode(OpCode.Ld_I1, (byte)5);
//        //    builder.EmitOpCode(OpCode.Cast_I1, (byte)TypeCode.U32);
//        //    builder.EmitOpCode(OpCode.Ret);

//        //    // Execute
//        //    __memory.InitStack();
//        //    IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//        //    // Check for expected value
//        //    Assert.AreEqual(5U, __interpreter.FetchValue<uint>());
//        //    Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        //}

//        [TestMethod()]
//        public void Convert_I1_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I1, (byte)5);
//            builder.EmitOpCode(OpCode.Cast_I1, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_I1_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I1, (byte)5);
//            builder.EmitOpCode(OpCode.Cast_I1, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_I1_F4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I1, (byte)5);
//            builder.EmitOpCode(OpCode.Cast_I1, (byte)TypeCode.Float);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5f, __interpreter.FetchValue<float>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_I1_F8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I1, (byte)5);
//            builder.EmitOpCode(OpCode.Cast_I1, (byte)TypeCode.Double);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5d, __interpreter.FetchValue<double>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }




//        [TestMethod()]
//        public void Convert_I2_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I2, (short)5);
//            builder.EmitOpCode(OpCode.Cast_I2, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_I2_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I2, (short)5);
//            builder.EmitOpCode(OpCode.Cast_I2, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_I2_F4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I2, (short)5);
//            builder.EmitOpCode(OpCode.Cast_I2, (byte)TypeCode.Float);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5f, __interpreter.FetchValue<float>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_I2_F8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I2, (short)5);
//            builder.EmitOpCode(OpCode.Cast_I2, (byte)TypeCode.Double);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5d, __interpreter.FetchValue<double>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Convert_I4_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.Cast_I4, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_I4_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.Cast_I4, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_I4_F4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.Cast_I4, (byte)TypeCode.Float);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5f, __interpreter.FetchValue<float>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_I4_F8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.Cast_I4, (byte)TypeCode.Double);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5d, __interpreter.FetchValue<double>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Convert_F4_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4, 5f);
//            builder.EmitOpCode(OpCode.Cast_F4, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_F4_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4, 5f);
//            builder.EmitOpCode(OpCode.Cast_F4, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_F4_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4, 5f);
//            builder.EmitOpCode(OpCode.Cast_F4, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_F4_F4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4, 5f);
//            builder.EmitOpCode(OpCode.Cast_F4, (byte)TypeCode.Float);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5f, __interpreter.FetchValue<float>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_F4_F8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4, 5f);
//            builder.EmitOpCode(OpCode.Cast_F4, (byte)TypeCode.Double);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5d, __interpreter.FetchValue<double>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Convert_F8_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F8, 5d);
//            builder.EmitOpCode(OpCode.Cast_F8, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_F8_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F8, 5d);
//            builder.EmitOpCode(OpCode.Cast_F8, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_F8_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F8, 5d);
//            builder.EmitOpCode(OpCode.Cast_F8, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_F8_F4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F8, 5d);
//            builder.EmitOpCode(OpCode.Cast_F8, (byte)TypeCode.Float);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5f, __interpreter.FetchValue<float>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Convert_F8_F8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F8, 5d);
//            builder.EmitOpCode(OpCode.Cast_F8, (byte)TypeCode.Double);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5d, __interpreter.FetchValue<double>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }


//    }
//}
