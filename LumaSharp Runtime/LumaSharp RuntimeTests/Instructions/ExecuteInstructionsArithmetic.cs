//using LumaSharp.Runtime.Emit;
//using LumaSharp.Runtime;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using TypeCode = LumaSharp.Runtime.TypeCode;

//namespace LumaSharp_RuntimeTests.Instructions
//{
//    [TestClass]
//    public unsafe sealed class ExecuteInstructionsArithmetic
//    {
//        [TestMethod()]
//        public void Arithmetic_Add_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.Ld_I4, 2);
//            builder.EmitOpCode(OpCode.Add, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(7, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Add_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 5L);
//            builder.EmitOpCode(OpCode.Ld_I8, 2L);
//            builder.EmitOpCode(OpCode.Add, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(7L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Add_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 5L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Ld_I8, 2L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte) TypeCode.U8);
//            builder.EmitOpCode(OpCode.Add, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(7UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Add_Float()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
//            builder.EmitOpCode(OpCode.Ld_F4, 2.5f);
//            builder.EmitOpCode(OpCode.Add, (byte)TypeCode.Float);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(8f, __interpreter.FetchValue<float>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Add_Double()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F8, 5.5d);
//            builder.EmitOpCode(OpCode.Ld_F8, 2.5d);
//            builder.EmitOpCode(OpCode.Add, (byte)TypeCode.Double);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(8d, __interpreter.FetchValue<double>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Arithmetic_Sub_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.Ld_I4, 2);
//            builder.EmitOpCode(OpCode.Sub, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(3, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Sub_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 5L);
//            builder.EmitOpCode(OpCode.Ld_I8, 2L);
//            builder.EmitOpCode(OpCode.Sub, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(3L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Sub_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 5L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Ld_I8, 2L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Sub, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(3UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Sub_Float()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
//            builder.EmitOpCode(OpCode.Ld_F4, 2.5f);
//            builder.EmitOpCode(OpCode.Sub, (byte)TypeCode.Float);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(3f, __interpreter.FetchValue<float>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Sub_Double()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F8, 5.5d);
//            builder.EmitOpCode(OpCode.Ld_F8, 2.5d);
//            builder.EmitOpCode(OpCode.Sub, (byte)TypeCode.Double);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(3d, __interpreter.FetchValue<double>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Arithmetic_Mul_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.Ld_I4, 2);
//            builder.EmitOpCode(OpCode.Mul, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(10, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Mul_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 5L);
//            builder.EmitOpCode(OpCode.Ld_I8, 2L);
//            builder.EmitOpCode(OpCode.Mul, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(10L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Mul_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 5L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Ld_I8, 2L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Mul, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(10UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Mul_Float()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
//            builder.EmitOpCode(OpCode.Ld_F4, 2.5f);
//            builder.EmitOpCode(OpCode.Mul, (byte)TypeCode.Float);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(13.75f, __interpreter.FetchValue<float>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Mul_Double()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F8, 5.5d);
//            builder.EmitOpCode(OpCode.Ld_F8, 2.5d);
//            builder.EmitOpCode(OpCode.Mul, (byte)TypeCode.Double);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(13.75d, __interpreter.FetchValue<double>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Arithmetic_Div_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 6);
//            builder.EmitOpCode(OpCode.Ld_I4, 2);
//            builder.EmitOpCode(OpCode.Div, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(3, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Div_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 6L);
//            builder.EmitOpCode(OpCode.Ld_I8, 2L);
//            builder.EmitOpCode(OpCode.Div, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(3L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Div_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 6L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Ld_I8, 2L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Div, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(3UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Div_Float()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4, 11f);
//            builder.EmitOpCode(OpCode.Ld_F4, 2f);
//            builder.EmitOpCode(OpCode.Div, (byte)TypeCode.Float);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5.5f, __interpreter.FetchValue<float>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Div_Double()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F8, 11d);
//            builder.EmitOpCode(OpCode.Ld_F8, 2d);
//            builder.EmitOpCode(OpCode.Div, (byte)TypeCode.Double);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5.5d, __interpreter.FetchValue<double>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Arithmetic_Neg_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 6);
//            builder.EmitOpCode(OpCode.Neg, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(-6, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Neg_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 6L);
//            builder.EmitOpCode(OpCode.Neg, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(-6L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Neg_Float()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4, 11f);
//            builder.EmitOpCode(OpCode.Neg, (byte)TypeCode.Float);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(-11f, __interpreter.FetchValue<float>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Neg_Double()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F8, 11d);
//            builder.EmitOpCode(OpCode.Neg, (byte)TypeCode.Double);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(-11d, __interpreter.FetchValue<double>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Arithmetic_Mod_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 4);
//            builder.EmitOpCode(OpCode.Ld_I4, 3);
//            builder.EmitOpCode(OpCode.Mod, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(1, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Mod_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 4L);
//            builder.EmitOpCode(OpCode.Ld_I8, 3L);
//            builder.EmitOpCode(OpCode.Mod, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(1L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Mod_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 4L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Ld_I8, 3L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Mod, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(1UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Mod_Float()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
            
//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4, 4f);
//            builder.EmitOpCode(OpCode.Ld_F4, 3f);
//            builder.EmitOpCode(OpCode.Mod, (byte)TypeCode.Float);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(1f, __interpreter.FetchValue<float>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Mod_Double()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F8, 4d);
//            builder.EmitOpCode(OpCode.Ld_F8, 3d);
//            builder.EmitOpCode(OpCode.Mod, (byte)TypeCode.Double);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(1d, __interpreter.FetchValue<double>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Arithmetic_And_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 4);
//            builder.EmitOpCode(OpCode.Ld_I4, 7);
//            builder.EmitOpCode(OpCode.And, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(4, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_And_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 4L);
//            builder.EmitOpCode(OpCode.Ld_I8, 7L);
//            builder.EmitOpCode(OpCode.And, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(4L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_And_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 4L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Ld_I8, 7L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.And, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(4UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Arithmetic_Or_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 4);
//            builder.EmitOpCode(OpCode.Ld_I4, 3);
//            builder.EmitOpCode(OpCode.Or, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(7, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Or_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 4L);
//            builder.EmitOpCode(OpCode.Ld_I8, 3L);
//            builder.EmitOpCode(OpCode.Or, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(7L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Or_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 4L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Ld_I8, 3L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Or, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(7UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Arithmetic_XOr_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 4);
//            builder.EmitOpCode(OpCode.Ld_I4, 7);
//            builder.EmitOpCode(OpCode.XOr, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(3, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_XOr_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 4L);
//            builder.EmitOpCode(OpCode.Ld_I8, 7L);
//            builder.EmitOpCode(OpCode.XOr, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(3L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_XOr_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 4L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Ld_I8, 7L);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.XOr, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(3UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Arithmetic_Not_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.Not, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(-6, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Not_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 10L);
//            builder.EmitOpCode(OpCode.Not, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(-11L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_Not_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, ulong.MaxValue);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Not, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(0UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Arithmetic_ShiftLeft_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 1);
//            builder.EmitOpCode(OpCode.Ld_I4, 3);
//            builder.EmitOpCode(OpCode.Bit_Shl, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(8, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_ShiftLeft_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 1L);
//            builder.EmitOpCode(OpCode.Ld_I4, 3);
//            builder.EmitOpCode(OpCode.Bit_Shl, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(8L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_ShiftLeft_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 1UL);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Ld_I4, 3);
//            builder.EmitOpCode(OpCode.Bit_Shl, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(8UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }



//        [TestMethod()]
//        public void Arithmetic_ShiftRight_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 8);
//            builder.EmitOpCode(OpCode.Ld_I4, 3);
//            builder.EmitOpCode(OpCode.Bit_Shr, (byte)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(1, __interpreter.FetchValue<int>());
//            Assert.AreEqual(__memory.stackBasePtr + 4, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_ShiftRight_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 8L);
//            builder.EmitOpCode(OpCode.Ld_I4, 3);
//            builder.EmitOpCode(OpCode.Bit_Shr, (byte)TypeCode.I64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(1L, __interpreter.FetchValue<long>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }

//        [TestMethod()]
//        public void Arithmetic_ShiftRight_U8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 8UL);
//            builder.EmitOpCode(OpCode.Cast_I8, (byte)TypeCode.U8);
//            builder.EmitOpCode(OpCode.Ld_I4, 3);
//            builder.EmitOpCode(OpCode.Bit_Shr, (byte)TypeCode.U64);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            IntPtr stackPtr = __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(1UL, __interpreter.FetchValue<ulong>());
//            Assert.AreEqual(__memory.stackBasePtr + 8, stackPtr);
//        }
//    }
//}
