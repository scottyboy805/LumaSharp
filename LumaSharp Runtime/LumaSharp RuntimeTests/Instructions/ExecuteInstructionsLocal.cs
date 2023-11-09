using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp_RuntimeTests.Instructions
{
    [TestClass]
    public unsafe sealed class ExecuteInstructionsLocal
    {
        [TestMethod()]
        public void Local_Store_0()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc_0);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            byte* stackPtr = null;
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    InstructionPtr = instructionPtr,
                    StackPtrOffset = 4,
                    ArgLocals = new _StackHandle[] { new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4} } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(4));
            Assert.AreEqual(__memory.stackBasePtr + 4, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Local_Store_1()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc_1);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            byte* stackPtr = null;
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    InstructionPtr = instructionPtr,
                    StackPtrOffset = 8,
                    ArgLocals = new _StackHandle[] 
                    { 
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
                    },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(4));
            Assert.AreEqual(__memory.stackBasePtr + 8, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Local_Store_2()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc_2);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            byte* stackPtr = null;
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    InstructionPtr = instructionPtr,
                    StackPtrOffset = 12,
                    ArgLocals = new _StackHandle[]
                    {
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 8 },
                    },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(8));
            Assert.AreEqual(__memory.stackBasePtr + 12, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Local_Store()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc, (byte)3);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            byte* stackPtr = null;
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    InstructionPtr = instructionPtr,
                    StackPtrOffset = 16,
                    ArgLocals = new _StackHandle[]
                    {
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 8 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 12 },
                    },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(12));
            Assert.AreEqual(__memory.stackBasePtr + 16, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Local_Store_E()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc_E, (ushort)3);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            byte* stackPtr = null;
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    InstructionPtr = instructionPtr,
                    StackPtrOffset = 16,
                    ArgLocals = new _StackHandle[]
                    {
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 8 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 12 },
                    },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
                Assert.AreEqual(__memory.stackBasePtr + 16, (IntPtr)stackPtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(12));
        }

        [TestMethod()]
        public void Local_Load_0()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc_0);
            builder.EmitOpCode(OpCode.Ld_Loc_0);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            byte* stackPtr = null;
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    InstructionPtr = instructionPtr,
                    StackPtrOffset = 4,
                    ArgLocals = new _StackHandle[] { new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } } },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(4));
            Assert.AreEqual(__memory.stackBasePtr + 8, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Local_Load_1()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc_1);
            builder.EmitOpCode(OpCode.Ld_Loc_1);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            byte* stackPtr = null;
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    InstructionPtr = instructionPtr,
                    StackPtrOffset = 8,
                    ArgLocals = new _StackHandle[]
                    {
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
                    },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(8));
            Assert.AreEqual(__memory.stackBasePtr + 12, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Local_Load_2()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc_2);
            builder.EmitOpCode(OpCode.Ld_Loc_2);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            byte* stackPtr = null;
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    InstructionPtr = instructionPtr,
                    StackPtrOffset = 12,
                    ArgLocals = new _StackHandle[]
                    {
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 8 },
                    },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(12));
            Assert.AreEqual(__memory.stackBasePtr + 16, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Local_Load()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc, (byte)3);
            builder.EmitOpCode(OpCode.Ld_Loc, (byte)3);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            byte* stackPtr = null;
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    InstructionPtr = instructionPtr,
                    StackPtrOffset = 16,
                    ArgLocals = new _StackHandle[]
                    {
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 8 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 12 },
                    },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
                Assert.AreEqual(__memory.stackBasePtr + 20, (IntPtr)stackPtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(16));
        }

        [TestMethod()]
        public void Local_Load_E()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc_E, (short)3);
            builder.EmitOpCode(OpCode.Ld_Loc_E, (short)3);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            byte* stackPtr = null;
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    InstructionPtr = instructionPtr,
                    StackPtrOffset = 16,
                    ArgLocals = new _StackHandle[]
                    {
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 8 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 12 },
                    },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(5, __interpreter.FetchValue<int>(16));
            Assert.AreEqual(__memory.stackBasePtr + 20, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Local_Load_Addr()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc, (byte)3);
            builder.EmitOpCode(OpCode.Ld_Loc_A, (byte)3);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            byte* stackPtr = null;
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    InstructionPtr = instructionPtr,
                    StackPtrOffset = 16,
                    ArgLocals = new _StackHandle[]
                    {
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 8 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 12 },
                    },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(__memory.stackBasePtr + 12, __interpreter.FetchValue<IntPtr>(16));
            Assert.AreEqual(__memory.stackBasePtr + 24, (IntPtr)stackPtr);
        }

        [TestMethod()]
        public void Local_Load_Addr_E()
        {
            // Create builder
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

            // Emit instruction
            builder.EmitOpCode(OpCode.Ld_I4, 5);
            builder.EmitOpCode(OpCode.St_Loc_E, (ushort)3);
            builder.EmitOpCode(OpCode.Ld_Loc_EA, (ushort)3);
            builder.EmitOpCode(OpCode.Ret);

            // Execute
            byte* stackPtr = null;
            __memory.InitStack();
            fixed (byte* instructionPtr = stream.ToArray())
            {
                _MethodHandle handle = new _MethodHandle
                {
                    InstructionPtr = instructionPtr,
                    StackPtrOffset = 16,
                    ArgLocals = new _StackHandle[]
                    {
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 8 },
                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 12 },
                    },
                };

                stackPtr = __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
            }

            // Check for expected value
            Assert.AreEqual(__memory.stackBasePtr + 12, __interpreter.FetchValue<IntPtr>(16));
            Assert.AreEqual(__memory.stackBasePtr + 24, (IntPtr)stackPtr);
        }
    }
}
