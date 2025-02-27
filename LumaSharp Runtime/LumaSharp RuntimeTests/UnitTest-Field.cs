using LumaSharp.Runtime;
using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime.Handle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppContext = LumaSharp.Runtime.AppContext;

namespace LumaSharp_RuntimeTests
{
    [TestClass]
    public class UnitTest_Field
    {
        [TestMethod]
        public unsafe void TestReadWrite()
        {
            BytecodeGenerator gen = new BytecodeGenerator();

            // Emit instructions
            gen.EmitToken(OpCode.New, 110);
            gen.Emit(OpCode.St_Var_0);
            gen.Emit(OpCode.Ld_Var_0);
            gen.Emit(OpCode.Ld_I4, 1234);
            gen.EmitToken(OpCode.St_Fld, 120);
            gen.Emit(OpCode.Ld_Var_0);
            gen.EmitToken(OpCode.Ld_Fld, 120);
            gen.Emit(OpCode.Ret);

            // Generate method
            _MethodHandle method = gen.GenerateMethod(new[] { RuntimeTypeCode.I32 }, new[] { RuntimeTypeCode.I32 }, 4);
            byte* instructions = gen.GenerateBytecode();

            // Create app and thread context
            AppContext appContext = new AppContext();
            ThreadContext threadContext = new ThreadContext(appContext);

            // Create type handle
            _TypeHandle typeHandle = new _TypeHandle(110, 4);
            _FieldHandle fieldHandle = new _FieldHandle(120, 0, new _TypeHandle(RuntimeTypeCode.I32));
            appContext.typeHandles[110] = (IntPtr)(&typeHandle);
            appContext.fieldHandles[120] = (IntPtr)(&fieldHandle);

            // Execute bytecode
            StackData* spReturn = __interpreter.ExecuteBytecode(threadContext, method, instructions);

            Assert.AreEqual(1234, spReturn->I32);
        }

        [TestMethod]
        public unsafe void TestWriteAddress()
        {
            BytecodeGenerator gen = new BytecodeGenerator();

            // Emit instructions
            gen.EmitToken(OpCode.New, 110);
            gen.Emit(OpCode.St_Var_0);
            gen.Emit(OpCode.Ld_Var_0);
            gen.EmitToken(OpCode.Ld_Fld_A, 120);
            gen.Emit(OpCode.Ld_I4, 1234);
            gen.Emit(OpCode.St_Addr);
            gen.Emit(OpCode.Ld_Var_0);
            gen.EmitToken(OpCode.Ld_Fld, 120);
            gen.Emit(OpCode.Ret);

            // Generate method
            _MethodHandle method = gen.GenerateMethod(new[] { RuntimeTypeCode.I32 }, new[] { RuntimeTypeCode.I32 }, 4);
            byte* instructions = gen.GenerateBytecode();

            // Create app and thread context
            AppContext appContext = new AppContext();
            ThreadContext threadContext = new ThreadContext(appContext);

            // Create type handle
            _TypeHandle typeHandle = new _TypeHandle(110, 4);
            _FieldHandle fieldHandle = new _FieldHandle(120, 0, new _TypeHandle(RuntimeTypeCode.I32));
            appContext.typeHandles[110] = (IntPtr)(&typeHandle);
            appContext.fieldHandles[120] = (IntPtr)(&fieldHandle);

            // Execute bytecode
            StackData* spReturn = __interpreter.ExecuteBytecode(threadContext, method, instructions);

            Assert.AreEqual(1234, spReturn->I32);
        }
    }
}
