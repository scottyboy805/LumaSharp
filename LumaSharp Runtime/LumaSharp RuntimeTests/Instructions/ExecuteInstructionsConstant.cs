//using LumaSharp.Runtime;
//using LumaSharp.Runtime.Emit;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace LumaSharp_RuntimeTests.Instructions
//{
//    [TestClass]
//    public sealed class ExecuteInstructionsConstant
//    {
//        [TestMethod()]
//        public void LoadConstant_I1()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I1, (byte)5);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual((int)5, __interpreter.FetchValue<int>());
//        }

//        [TestMethod()]
//        public void LoadConstant_I2()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I2, (short)5);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual((int)5, __interpreter.FetchValue<int>());
//        }

//        [TestMethod()]
//        public void LoadConstant_I4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5, __interpreter.FetchValue<int>());
//        }

//        [TestMethod()]
//        public void LoadConstant_I4_0()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4_0);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(0, __interpreter.FetchValue<int>());
//        }

//        [TestMethod()]
//        public void LoadConstant_I4_1()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4_1);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(1, __interpreter.FetchValue<int>());
//        }

//        [TestMethod()]
//        public void LoadConstant_I4_M1()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4_M1);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(-1, __interpreter.FetchValue<int>());
//        }

//        [TestMethod()]
//        public void LoadConstant_I8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I8, 50L);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(50L, __interpreter.FetchValue<long>());
//        }

//        [TestMethod()]
//        public void LoadConstant_F4()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5.5f, __interpreter.FetchValue<float>());
//        }

//        [TestMethod()]
//        public void LoadConstant_F4_0()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F4_0);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(0f, __interpreter.FetchValue<float>());
//        }

//        [TestMethod()]
//        public void LoadConstant_F8()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_F8, 5.8d);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Check for expected value
//            Assert.AreEqual(5.8d, __interpreter.FetchValue<double>());
//        }
//    }
//}
