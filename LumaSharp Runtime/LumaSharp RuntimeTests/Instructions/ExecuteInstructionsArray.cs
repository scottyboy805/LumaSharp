//using LumaSharp.Runtime.Emit;
//using LumaSharp.Runtime.Handle;
//using LumaSharp.Runtime;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using TypeCode = LumaSharp.Runtime.TypeCode;

//namespace LumaSharp_RuntimeTests.Instructions
//{
//    [TestClass]
//    public unsafe sealed class ExecuteInstructionsArray
//    {
//        [TestMethod()]
//        public void Array_Create_I32()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.NewArr_S, (int)TypeCode.I32);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            fixed (byte* instructionPtr = stream.ToArray())
//            {
//                _MethodHandle handle = new _MethodHandle
//                {
//                    InstructionPtr = instructionPtr,
//                    MaxStack = 8,
//                };

//                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
//            }

//            // Check for expected value (8 byte offset for maxstack, p[lus 12 byte offset for memory handle
//            Assert.AreEqual(__memory.stackBasePtr + 24, __interpreter.FetchValue<IntPtr>());
//        }

//        [TestMethod()]
//        public void Array_Store_I32_OutOfBounds()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 123); // Value to store
//            builder.EmitOpCode(OpCode.Ld_I4, 5); // Array index
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.NewArr_S, (int)TypeCode.I32);
//            builder.EmitOpCode(OpCode.St_Elem);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            fixed (byte* instructionPtr = stream.ToArray())
//            {
//                _MethodHandle handle = new _MethodHandle
//                {
//                    InstructionPtr = instructionPtr,
//                    MaxStack = 16,
//                };

//                Assert.ThrowsException<IndexOutOfRangeException>(() =>
//                {
//                    __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
//                });
//            }
//        }

//        [TestMethod()]
//        public void Array_Store_I32()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 123); // Value to store
//            builder.EmitOpCode(OpCode.Ld_I4, 0); // Array index
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.NewArr_S, (int)TypeCode.I32);
//            builder.EmitOpCode(OpCode.St_Elem);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            fixed (byte* instructionPtr = stream.ToArray())
//            {
//                _MethodHandle handle = new _MethodHandle
//                {
//                    InstructionPtr = instructionPtr,
//                    MaxStack = 16,
//                };

//                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
//            }

//            // Check for expected value (16 byte offset for maxstack, plus 16 byte offset for memory handle
//            Assert.AreEqual(123, __interpreter.FetchValue<int>(32));
//        }

//        [TestMethod()]
//        public void Array_Store_I32_Index()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 123); // Value to store
//            builder.EmitOpCode(OpCode.Ld_I4, 2); // Array index
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.NewArr_S, (int)TypeCode.I32);
//            builder.EmitOpCode(OpCode.St_Elem);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            fixed (byte* instructionPtr = stream.ToArray())
//            {
//                _MethodHandle handle = new _MethodHandle
//                {
//                    InstructionPtr = instructionPtr,
//                    MaxStack = 16,
//                };

//                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
//            }

//            // Check for expected value (16 byte offset for maxstack, plus 16 byte offset for memory handle, plus 8 byte index 2 offset
//            Assert.AreEqual(123, __interpreter.FetchValue<int>(40));
//        }

//        [TestMethod()]
//        public void Array_Load_I32()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 123); // Value to store
//            builder.EmitOpCode(OpCode.Ld_I4, 0); // Array index
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.NewArr_S, (int)TypeCode.I32);
//            builder.EmitOpCode(OpCode.St_Loc_0);
//            builder.EmitOpCode(OpCode.Ld_Loc_0);
//            builder.EmitOpCode(OpCode.St_Elem);

//            builder.EmitOpCode(OpCode.Ld_I4, 0);
//            builder.EmitOpCode(OpCode.Ld_Loc_0);
//            builder.EmitOpCode(OpCode.Ld_Elem);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            fixed (byte* instructionPtr = stream.ToArray())
//            {
//                _MethodHandle handle = new _MethodHandle
//                {
//                    InstructionPtr = instructionPtr,
//                    MaxStack = 16,
//                    StackEvalOffset = 8,
//                    ArgLocals = new _StackHandle[] { new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 8 } } },
//                };

//                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
//            }

//            // Check for expected value (16 byte offset for maxstack, 8 byte offset for locals, plus 16 byte offset for array handle
//            Assert.AreEqual(123, __interpreter.FetchValue<int>(40));
//        }

//        [TestMethod()]
//        public void Array_Load_I32_Index()
//        {
//            // Create builder
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));

//            // Emit instruction
//            builder.EmitOpCode(OpCode.Ld_I4, 123); // Value to store
//            builder.EmitOpCode(OpCode.Ld_I4, 2); // Array index
//            builder.EmitOpCode(OpCode.Ld_I4, 5);
//            builder.EmitOpCode(OpCode.NewArr_S, (int)TypeCode.I32);
//            builder.EmitOpCode(OpCode.St_Loc_0);
//            builder.EmitOpCode(OpCode.Ld_Loc_0);
//            builder.EmitOpCode(OpCode.St_Elem);

//            builder.EmitOpCode(OpCode.Ld_I4, 2);
//            builder.EmitOpCode(OpCode.Ld_Loc_0);
//            builder.EmitOpCode(OpCode.Ld_Elem);
//            builder.EmitOpCode(OpCode.Ret);

//            // Execute
//            __memory.InitStack();
//            fixed (byte* instructionPtr = stream.ToArray())
//            {
//                _MethodHandle handle = new _MethodHandle
//                {
//                    InstructionPtr = instructionPtr,
//                    MaxStack = 16,
//                    StackEvalOffset = 8,
//                    ArgLocals = new _StackHandle[] { new _StackHandle { TypeHandle = new _TypeHandle { TypeSize = 8 } } },
//                };

//                __interpreter.ExecuteBytecode(handle, (byte*)__memory.stackBasePtr);
//            }

//            // Check for expected value (16 byte offset for maxstack, 8 byte offset for locals, plus 16 byte offset for array handle, plus 8 byte index 2 offset
//            Assert.AreEqual(123, __interpreter.FetchValue<int>(48));
//        }
//    }
//}
