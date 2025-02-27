﻿//using LumaSharp.Runtime.Emit;
//using LumaSharp.Runtime.Handle;
//using LumaSharp.Runtime;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace LumaSharp_RuntimeTests.Instructions
//{
//    [TestClass]
//    public unsafe sealed class ExecuteInstructionsArgument
//    {
//        [TestMethod()]
//        public void Argument_Store_0()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.St_Arg_0);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            fixed (byte* instructionPtr = stream.ToArray())
//            {
//                _MethodHandle handle = new _MethodHandle
//                {
//                    InstructionPtr = instructionPtr,
//                    StackEvalOffset = 4,
//                    ArgLocals = new _StackHandle[] { new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } } },
//                };

//                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
//            }

//            // Check for expected value
//            Assert.AreEqual(5, __interpreter.FetchValue<int>(4));
//        }

//        [TestMethod()]
//        public void Argument_Store_1()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.St_Arg_1);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            fixed (byte* instructionPtr = stream.ToArray())
//            {
//                _MethodHandle handle = new _MethodHandle
//                {
//                    InstructionPtr = instructionPtr,
//                    StackEvalOffset = 8,
//                    ArgLocals = new _StackHandle[]
//                    {
//                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
//                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
//                    },
//                };

//                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
//            }

//            // Check for expected value
//            Assert.AreEqual(5, __interpreter.FetchValue<int>(4));
//        }

//        [TestMethod()]
//        public void Argument_Store_2()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.St_Arg_2);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            fixed (byte* instructionPtr = stream.ToArray())
//            {
//                _MethodHandle handle = new _MethodHandle
//                {
//                    InstructionPtr = instructionPtr,
//                    StackEvalOffset = 12,
//                    ArgLocals = new _StackHandle[]
//                    {
//                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
//                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
//                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 8 },
//                    },
//                };

//                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
//            }

//            // Check for expected value
//            Assert.AreEqual(5, __interpreter.FetchValue<int>(8));
//        }

//        [TestMethod()]
//        public void Argument_Load_0()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.St_Arg_0);
//            builder.EmitOpCode(OpCode.Ld_Arg_0);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            fixed (byte* instructionPtr = stream.ToArray())
//            {
//                _MethodHandle handle = new _MethodHandle
//                {
//                    InstructionPtr = instructionPtr,
//                    StackEvalOffset = 4,
//                    ArgLocals = new _StackHandle[] { new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } } },
//                };

//                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
//            }

//            // Check for expected value
//            Assert.AreEqual(5, __interpreter.FetchValue<int>(4));
//        }

//        [TestMethod()]
//        public void Argument_Load_1()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.St_Arg_1);
//            builder.EmitOpCode(OpCode.Ld_Arg_0);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            fixed (byte* instructionPtr = stream.ToArray())
//            {
//                _MethodHandle handle = new _MethodHandle
//                {
//                    InstructionPtr = instructionPtr,
//                    StackEvalOffset = 8,
//                    ArgLocals = new _StackHandle[]
//                    {
//                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
//                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
//                    },
//                };

//                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
//            }

//            // Check for expected value
//            Assert.AreEqual(5, __interpreter.FetchValue<int>(8));
//        }

//        [TestMethod()]
//        public void Argument_Load_2()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.St_Arg_2);
//            builder.EmitOpCode(OpCode.Ld_Arg_0);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            fixed (byte* instructionPtr = stream.ToArray())
//            {
//                _MethodHandle handle = new _MethodHandle
//                {
//                    InstructionPtr = instructionPtr,
//                    StackEvalOffset = 12,
//                    ArgLocals = new _StackHandle[]
//                    {
//                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 } },
//                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 4 },
//                        new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 4 }, StackOffset = 8 },
//                    },
//                };

//                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
//            }

//            // Check for expected value
//            Assert.AreEqual(5, __interpreter.FetchValue<int>(12));
//        }
//    }
//}
