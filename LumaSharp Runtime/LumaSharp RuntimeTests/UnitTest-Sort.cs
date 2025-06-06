using LumaSharp.Runtime;
using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime.Handle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppContext = LumaSharp.Runtime.AppContext;
using RuntimeTypeCode = LumaSharp.Runtime.RuntimeTypeCode;

namespace LumaSharp_RuntimeTests
{
    [TestClass]
    public class UnitTest_Sort
    {
        [TestMethod]
        public unsafe void BubbleSort()
        {
            // Create builder
            BytecodeGenerator gen = new BytecodeGenerator();

            #region Instructions
            gen.Emit(OpCode.Nop);
            gen.Emit(OpCode.Ld_I4_0);
            gen.Emit(OpCode.St_Var_1);
            gen.Emit(OpCode.Ld_I4_0);
            gen.Emit(OpCode.St_Var_2);
            gen.Emit(OpCode.Jmp, 72);                // Jump
            {
                // Jump target 0x0007
                gen.Emit(OpCode.Nop);
                gen.Emit(OpCode.Ld_I4_0);
                gen.Emit(OpCode.St_Var_3);
                gen.Emit(OpCode.Jmp, 43);            // Jump
                {
                    // Jump target
                    gen.Emit(OpCode.Nop);
                    gen.Emit(OpCode.Ld_Var_0);
                    gen.Emit(OpCode.Ld_Var_3);
                    gen.Emit(OpCode.Ld_Elem);
                    gen.Emit(OpCode.Ld_Var_0);
                    gen.Emit(OpCode.Ld_Var_3);
                    gen.Emit(OpCode.Ld_I4_1);
                    gen.Emit(OpCode.Add);
                    gen.Emit(OpCode.Ld_Elem);
                    gen.Emit(OpCode.Cmp_G);
                    gen.Emit(OpCode.St_Var, (sbyte)4);
                    gen.Emit(OpCode.Ld_Var, (sbyte)4);
                    gen.Emit(OpCode.Jmp_0, 19);              // Jump

                    gen.Emit(OpCode.Nop);
                    gen.Emit(OpCode.Ld_Var_0);
                    gen.Emit(OpCode.Ld_Var_3);
                    gen.Emit(OpCode.Ld_I4_1);
                    gen.Emit(OpCode.Add);
                    gen.Emit(OpCode.Ld_Elem);
                    gen.Emit(OpCode.St_Var_1);
                    gen.Emit(OpCode.Ld_Var_0);
                    gen.Emit(OpCode.Ld_Var_3);
                    gen.Emit(OpCode.Ld_I4_1);
                    gen.Emit(OpCode.Add);
                    gen.Emit(OpCode.Ld_Var_0);
                    gen.Emit(OpCode.Ld_Var_3);
                    gen.Emit(OpCode.Ld_Elem);
                    gen.Emit(OpCode.St_Elem);
                    gen.Emit(OpCode.Ld_Var_0);
                    gen.Emit(OpCode.Ld_Var_3);
                    gen.Emit(OpCode.Ld_Var_1);
                    gen.Emit(OpCode.St_Elem);
                    
                    // Jump target
                    gen.Emit(OpCode.Nop);
                    gen.Emit(OpCode.Ld_Var_3);
                    gen.Emit(OpCode.Ld_I4_1);
                    gen.Emit(OpCode.Add);
                    gen.Emit(OpCode.St_Var_3);

                    // Jump target 0x0034
                    gen.Emit(OpCode.Ld_Var_3);
                    gen.Emit(OpCode.Ld_Var_0);
                    gen.Emit(OpCode.Ld_Len);
                    gen.Emit(OpCode.Cast_I4);
                    gen.Emit(OpCode.Ld_I4_1);
                    gen.Emit(OpCode.Sub);
                    gen.Emit(OpCode.Cmp_L);
                    gen.Emit(OpCode.St_Var, (sbyte)5);
                    gen.Emit(OpCode.Ld_Var, (sbyte)5);
                    gen.Emit(OpCode.Jmp_1, -58);
                }
                gen.Emit(OpCode.Nop);
                gen.Emit(OpCode.Ld_Var_2);
                gen.Emit(OpCode.Ld_I4_1);
                gen.Emit(OpCode.Add);
                gen.Emit(OpCode.St_Var_2);

                // Jump target 0x0047
                gen.Emit(OpCode.Ld_Var_2);
                gen.Emit(OpCode.Ld_Var_0);
                gen.Emit(OpCode.Ld_Len);
                gen.Emit(OpCode.Cast_I4);
                gen.Emit(OpCode.Cmp_L);
                gen.Emit(OpCode.St_Var, (sbyte)6);
                gen.Emit(OpCode.Ld_Var, (sbyte)6);
                gen.Emit(OpCode.Jmp_1, -86);
            }
            gen.Emit(OpCode.Ld_Var_0);
            gen.Emit(OpCode.St_Var, (sbyte)7);
            gen.Emit(OpCode.Jmp, 0);
            gen.Emit(OpCode.Ld_Var, (sbyte)7);
            gen.Emit(OpCode.Ret);
            #endregion


            // Generate method
            _MethodHandle* method = gen.GenerateMethod(new[] { RuntimeTypeCode.I32 }, new[] { RuntimeTypeCode.I32, RuntimeTypeCode.I32, RuntimeTypeCode.I32, RuntimeTypeCode.I32, RuntimeTypeCode.I32, RuntimeTypeCode.I32, RuntimeTypeCode.I32 }, 4);
           
            // Create app and thread context
            AppContext appContext = new AppContext();
            AssemblyContext asmContext = new AssemblyContext(appContext);
            ThreadContext threadContext = new ThreadContext(appContext);

            // Create test array
            _TypeHandle type = new _TypeHandle(RuntimeTypeCode.I32);
            int* arr =  (int*)__memory.AllocArray(&type, 5);
            arr[0] = 11;
            arr[1] = 32;
            arr[2] = 8;
            arr[3] = 17;
            arr[4] = 4;

            // Push arg
            StackData* spArg = (StackData*)threadContext.ThreadStackPtr;

            spArg->Type = StackTypeCode.Address;
            spArg->Ptr = (IntPtr)arr;

            // Execute bytecode
            StackData* spReturn = __interpreter.ExecuteBytecode(threadContext, asmContext, method);

            Assert.AreEqual(4, arr[0]);
            Assert.AreEqual(8, arr[1]);
            Assert.AreEqual(11, arr[2]);
            Assert.AreEqual(17, arr[3]);
            Assert.AreEqual(32, arr[4]);
        }
    }
}
