using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_RuntimeTests.Instructions
{
    [TestClass]
    public unsafe sealed class ExecuteInstructionsIndirect
    {
        [TestMethod()]
        public void Indirect_Load_I1()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc_0);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)0);
            builder.EmitOpCode(OpCode.Ld_Addr_I1);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            byte* stackPtr = null;
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    stackPtrOffset = 4,
                    argLocals = new _StackHandle[] { new _StackHandle { typeHandle = new _TypeHandle { size = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(4));
            Assert.AreEqual(__memory.stackBasePtr + 8, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Indirect_Load_I2()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc_0);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)0);
            builder.EmitOpCode(OpCode.Ld_Addr_I2);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            byte* stackPtr = null;
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    stackPtrOffset = 4,
                    argLocals = new _StackHandle[] { new _StackHandle { typeHandle = new _TypeHandle { size = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(4));
            Assert.AreEqual(__memory.stackBasePtr + 8, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Indirect_Load_I4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc_0);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)0);
            builder.EmitOpCode(OpCode.Ld_Addr_I4);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            byte* stackPtr = null;
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    stackPtrOffset = 4,
                    argLocals = new _StackHandle[] { new _StackHandle { typeHandle = new _TypeHandle { size = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(4));
            Assert.AreEqual(__memory.stackBasePtr + 8, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Indirect_Load_I8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.St_Loc_0);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)0);
            builder.EmitOpCode(OpCode.Ld_Addr_I8);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            byte* stackPtr = null;
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    stackPtrOffset = 4,
                    argLocals = new _StackHandle[] { new _StackHandle { typeHandle = new _TypeHandle { size = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5L, __interpreter.FetchValue<long>(4));
            Assert.AreEqual(__memory.stackBasePtr + 16, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Indirect_Load_F4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.St_Loc_0);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)0);
            builder.EmitOpCode(OpCode.Ld_Addr_F4);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            byte* stackPtr = null;
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    stackPtrOffset = 4,
                    argLocals = new _StackHandle[] { new _StackHandle { typeHandle = new _TypeHandle { size = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5.5f, __interpreter.FetchValue<float>(4));
            Assert.AreEqual(__memory.stackBasePtr + 8, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Indirect_Load_F8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F8, 5.5d);
            builder.EmitOpCode(OpCode.St_Loc_0);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)0);
            builder.EmitOpCode(OpCode.Ld_Addr_F8);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            byte* stackPtr = null;
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    stackPtrOffset = 4,
                    argLocals = new _StackHandle[] { new _StackHandle { typeHandle = new _TypeHandle { size = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5.5d, __interpreter.FetchValue<double>(4));
            Assert.AreEqual(__memory.stackBasePtr + 16, (IntPtr)stackPtr);
        }



        [TestMethod()]
        public void Indirect_Store_I1()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)0);
            builder.EmitOpCode(OpCode.St_Addr_I1);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            byte* stackPtr = null;
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    stackPtrOffset = 4,
                    argLocals = new _StackHandle[] { new _StackHandle { typeHandle = new _TypeHandle { size = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(0));
            Assert.AreEqual(__memory.stackBasePtr + 4, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Indirect_Store_I2()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)0);
            builder.EmitOpCode(OpCode.St_Addr_I2);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            byte* stackPtr = null;
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    stackPtrOffset = 4,
                    argLocals = new _StackHandle[] { new _StackHandle { typeHandle = new _TypeHandle { size = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(0));
            Assert.AreEqual(__memory.stackBasePtr + 4, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Indirect_Store_I4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)0);
            builder.EmitOpCode(OpCode.St_Addr_I4);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            byte* stackPtr = null;
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    stackPtrOffset = 4,
                    argLocals = new _StackHandle[] { new _StackHandle { typeHandle = new _TypeHandle { size = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(0));
            Assert.AreEqual(__memory.stackBasePtr + 4, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Indirect_Store_I8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I8, 5L);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)0);
            builder.EmitOpCode(OpCode.St_Addr_I8);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            byte* stackPtr = null;
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    stackPtrOffset = 4,
                    argLocals = new _StackHandle[] { new _StackHandle { typeHandle = new _TypeHandle { size = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5L, __interpreter.FetchValue<long>(0));
            Assert.AreEqual(__memory.stackBasePtr + 4, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Indirect_Store_F4()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F4, 5.5f);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)0);
            builder.EmitOpCode(OpCode.St_Addr_F4);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            byte* stackPtr = null;
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    stackPtrOffset = 4,
                    argLocals = new _StackHandle[] { new _StackHandle { typeHandle = new _TypeHandle { size = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5.5f, __interpreter.FetchValue<float>(0));
            Assert.AreEqual(__memory.stackBasePtr + 4, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Indirect_Store_F8()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_F8, 5.5d);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)0);
            builder.EmitOpCode(OpCode.St_Addr_F8);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            __memory.InitStack();
            byte* stackPtr = null;
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    instructionPtr = instructionPtr,
                    stackPtrOffset = 4,
                    argLocals = new _StackHandle[] { new _StackHandle { typeHandle = new _TypeHandle { size = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5.5d, __interpreter.FetchValue<double>(0));
            Assert.AreEqual(__memory.stackBasePtr + 4, (IntPtr)stackPtr);
        }
    }
}
