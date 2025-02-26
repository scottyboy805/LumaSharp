using LumaSharp.Runtime;
using LumaSharp.Runtime.Handle;
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
            // 1 arg
            // 6 locals

            byte[] instructions =
            {
                0x00,       // Nop
                0x19,       // Ld_I4_0
                0x2A,       // St_Var_1
                0x1A,       // Ld_I4_1
                0x2B,       // St_Var_2
                0x19,       // Ld_I4_0
                0x2C,       // St_Var_3
                0x13, 2, 0x00, 0x00, 0x00,    // Ld_I4, 2
                0x2D, 4,    // St_Var, 4
                0xB3, 16, 0x00, 0x00, 0x00,       // Jump

                // Jump target:
                    0x00,       // Nop
                    0x22,       // Ld_Var_1
                    0x23,       // Ld_Var_2
                    0x71,       // Add
                    0x2C,       // St_Var_3
                    0x23,       // Ld_Var_2
                    0x2A,       // St_Var_1
                    0x24,       // Ld_Var_3
                    0x2B,       // St_Var_2
                    0x00,       // Nop
                    0x25, 4,    // Ld_Var, 4
                    0x1A,       // Ld_I4_1
                    0x71,       // Add
                    0x2D, 4,    // St_Var, 4

                    0x25, 4,    // Ld_Var, 4
                    0x21,       // Ld_Var_0     //   Argument
                    0x93,       // Cmp_Le
                    0x2D, 5,    // St_Var, 5
                    0x25, 5,    // Ld_Var, 5
                    0xB1, 0x00, 0x00, 0x00, 0x00,      // Jmp_1

                0x24,       // Ld_Var_3
                0x2D, 6,    // St_Var, 6
                0xB3, 0x00, 0x00, 0x00, 0x00,           // Jmp
                0x25, 6,    // Ld_Var, 6
                0xFD,       // Ret
            };

            _VariableHandle arg = new _VariableHandle(RuntimeTypeCode.I4, 0);

            _VariableHandle loc0 = new _VariableHandle(RuntimeTypeCode.I4, 4);
            _VariableHandle loc1 = new _VariableHandle(RuntimeTypeCode.I4, 8);
            _VariableHandle loc2 = new _VariableHandle(RuntimeTypeCode.I4, 12);
            _VariableHandle loc3 = new _VariableHandle(RuntimeTypeCode.I4, 16);
            _VariableHandle loc4 = new _VariableHandle(RuntimeTypeCode.I4, 20);
            _VariableHandle loc5 = new _VariableHandle(RuntimeTypeCode.I4, 24);


            _MethodBodyHandle bodyHandle = new _MethodBodyHandle(16, null, 7);
            _MethodSignature signatureHandle = new _MethodSignature(1, null, null);
            _MethodHandle methodHandle = new _MethodHandle(0, signatureHandle, bodyHandle);

            // Create app and thread context
            AppContext appContext = new AppContext();
            ThreadContext threadContext = new ThreadContext(appContext);

            // Pin instruction memory
            fixed(byte* inst = instructions)
            {
                (*(int*)(inst + 44)) = -29;

                // Push arg
                StackData* spArg = (StackData*)threadContext.ThreadStackPtr;

                spArg->Type = StackTypeCode.I32;
                spArg->I32 = 8;

                // Execute bytecode
                StackData* spReturn = __interpreter.ExecuteBytecode(threadContext, methodHandle, inst);

                Assert.AreEqual(21, spReturn->I32);
            }
        }
    }
}
