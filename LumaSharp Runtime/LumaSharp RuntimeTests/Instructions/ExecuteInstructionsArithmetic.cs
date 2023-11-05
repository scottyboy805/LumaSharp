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
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(7, __interpreter.FetchValue<int>());
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
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(7L, __interpreter.FetchValue<long>());
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
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(7UL, __interpreter.FetchValue<ulong>());
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
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(8f, __interpreter.FetchValue<float>());
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
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Check for expected value
            Assert.AreEqual(8d, __interpreter.FetchValue<double>());
        }
    }
}
