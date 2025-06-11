using LumaSharp.Runtime;
using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime.Reader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppContext = LumaSharp.Runtime.AppContext;
using RuntimeTypeCode = LumaSharp.Runtime.RuntimeTypeCode;

namespace LumaSharp_RuntimeTests
{
    [TestClass]
    public class UnitTest_Fibonacci
    {
        [TestMethod]
        public unsafe void TestIterative()
        {
            BytecodeGenerator gen = new BytecodeGenerator();

            gen.Emit(OpCode.Nop);
            gen.Emit(OpCode.Ld_I4_0);
            gen.Emit(OpCode.St_Var_1);
            gen.Emit(OpCode.Ld_I4_1);
            gen.Emit(OpCode.St_Var_2);
            gen.Emit(OpCode.Ld_I4_0);
            gen.Emit(OpCode.St_Var_3);
            gen.Emit(OpCode.Ld_I4, 2);
            gen.Emit(OpCode.St_Var, (sbyte)4);
            gen.Emit(OpCode.Jmp, 16);

            gen.Emit(OpCode.Nop);
            gen.Emit(OpCode.Ld_Var_1);
            gen.Emit(OpCode.Ld_Var_2);
            gen.Emit(OpCode.Add);
            gen.Emit(OpCode.St_Var_3);
            gen.Emit(OpCode.Ld_Var_2);
            gen.Emit(OpCode.St_Var_1);
            gen.Emit(OpCode.Ld_Var_3);
            gen.Emit(OpCode.St_Var_2);
            gen.Emit(OpCode.Nop);
            gen.Emit(OpCode.Ld_Var, (sbyte)4);
            gen.Emit(OpCode.Ld_I4_1);
            gen.Emit(OpCode.Add);
            gen.Emit(OpCode.St_Var, (sbyte)4);

            gen.Emit(OpCode.Ld_Var, (sbyte)4);
            gen.Emit(OpCode.Ld_Var_0);
            gen.Emit(OpCode.Cmp_Le);
            gen.Emit(OpCode.St_Var, (sbyte)5);
            gen.Emit(OpCode.Ld_Var, (sbyte)5);
            gen.Emit(OpCode.Jmp_1, -29);

            gen.Emit(OpCode.Ld_Var_3);
            gen.Emit(OpCode.St_Var, (sbyte)6);
            gen.Emit(OpCode.Ld_Var, (sbyte)6);
            gen.Emit(OpCode.Ret);


            _MethodHandle* method = gen.GenerateMethod(new[] { RuntimeTypeCode.I32 }, new RuntimeTypeCode[] { RuntimeTypeCode.I32, RuntimeTypeCode.I32, RuntimeTypeCode.I32, RuntimeTypeCode.I32, RuntimeTypeCode.I32, RuntimeTypeCode.I32 }, 7);

            // Create app and thread context
            AppContext appContext = new AppContext();
            AssemblyContext asmContext = new AssemblyContext(appContext);
            ThreadContext threadContext = appContext.GetCurrentThreadContext();

            // Push arg
            StackData* spArg = (StackData*)threadContext.ThreadStackPtr;

            spArg->Type = StackTypeCode.I32;
            spArg->I32 = 8;

            // Execute bytecode
            StackData* spReturn = __interpreter.ExecuteBytecode(threadContext, asmContext, method);

            Assert.AreEqual(21, spReturn->I32);
        }

        [TestMethod]
        public unsafe void TestIterativeExternal()
        {
            // Read the bytecode
            BytecodeReader reader = new BytecodeReader(File.OpenText("../../../Data/FibonacciIterative.bytecode"));

            // Build the method
            _MethodHandle* method = reader.GenerateMethod();

            // Create app and thread context
            AppContext appContext = new AppContext();
            AssemblyContext asmContext = new AssemblyContext(appContext);
            ThreadContext threadContext = appContext.GetCurrentThreadContext();

            // Push arg
            StackData* spArg = (StackData*)threadContext.ThreadStackPtr;

            spArg->Type = StackTypeCode.I32;
            spArg->I32 = 8;

            // Execute bytecode
            StackData* spReturn = __interpreter.ExecuteBytecode(threadContext, asmContext, method);

            Assert.AreEqual(21, spReturn->I32);
        }

        [TestMethod]
        public unsafe void TestRecursive()
        {
            BytecodeGenerator gen = new BytecodeGenerator();

            gen.Emit(OpCode.Nop);
            gen.Emit(OpCode.Ld_Var_0);
            gen.Emit(OpCode.Ld_I4_0);
            gen.Emit(OpCode.Cmp_Eq);
            gen.Emit(OpCode.St_Var_1);
            gen.Emit(OpCode.Ld_Var_1);
            gen.Emit(OpCode.Jmp_0, 8);

            gen.Emit(OpCode.Nop);
            gen.Emit(OpCode.Ld_I4_1);
            gen.Emit(OpCode.St_Var_2);
            gen.Emit(OpCode.Jmp, 44);

            // Jump target 0x000e
            gen.Emit(OpCode.Ld_Var_0);
            gen.Emit(OpCode.Ld_I4_1);
            gen.Emit(OpCode.Cmp_Eq);
            gen.Emit(OpCode.St_Var_3);
            gen.Emit(OpCode.Ld_Var_3);
            gen.Emit(OpCode.Jmp_0, 8);

            // Jump Target 0x0016
            gen.Emit(OpCode.Nop);
            gen.Emit(OpCode.Ld_I4_1);
            gen.Emit(OpCode.St_Var_2);
            gen.Emit(OpCode.Jmp, 28);

            // Jump target 0x001b
            gen.Emit(OpCode.Nop);
            gen.Emit(OpCode.Ld_Var_0);
            gen.Emit(OpCode.Ld_I4, 2);
            gen.Emit(OpCode.Sub);
            gen.EmitToken(OpCode.Call, 110);
            gen.Emit(OpCode.Ld_Var_0);
            gen.Emit(OpCode.Ld_I4_1);
            gen.Emit(OpCode.Sub);
            gen.EmitToken(OpCode.Call, 110);
            gen.Emit(OpCode.Add);
            gen.Emit(OpCode.St_Var_2);
            gen.Emit(OpCode.Jmp, 0);

            // Jump target 0x0030
            gen.Emit(OpCode.Ld_Var_2);
            gen.Emit(OpCode.Ret);


            // Generate method
            _MethodHandle* method = gen.GenerateMethod(new[] { RuntimeTypeCode.I32 }, new[] { RuntimeTypeCode.Bool, RuntimeTypeCode.I32, RuntimeTypeCode.Bool }, 4);
            
            // Create app and thread context
            AppContext appContext = new AppContext();
            AssemblyContext asmContext = new AssemblyContext(appContext);
            ThreadContext threadContext = appContext.GetCurrentThreadContext();

            asmContext.methodHandles[110] = (IntPtr)method;

            // Push arg
            StackData arg = new StackData { Type = StackTypeCode.I32, I32 = 8 };

            // Execute bytecode
            StackData* spReturn = _MethodHandle.Invoke(threadContext, asmContext, method, IntPtr.Zero, &arg);

            Assert.AreEqual(34, spReturn->I32);
        }

        [TestMethod]
        public unsafe void TestRecursiveExternal()
        {
            // Read the bytecode
            BytecodeReader reader = new BytecodeReader(File.OpenText("../../../Data/FibonacciRecursive.bytecode"));

            // Build the method
            _MethodHandle* method = reader.GenerateMethod();

            // Create app and thread context
            AppContext appContext = new AppContext();
            AssemblyContext asmContext = new AssemblyContext(appContext);
            ThreadContext threadContext = appContext.GetCurrentThreadContext();

            asmContext.methodHandles[110] = (IntPtr)method;

            // Push arg
            StackData arg = new StackData { Type = StackTypeCode.I32, I32 = 8 };

            // Execute bytecode
            StackData* spReturn = _MethodHandle.Invoke(threadContext, asmContext, method, IntPtr.Zero, &arg);

            Assert.AreEqual(34, spReturn->I32);
        }
    }
}
