using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Emit.Builder;
using LumaSharp_Compiler.Semantics.Model;
using LumaSharp_Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Emit.IntegratedSource.Complete
{
    [TestClass]
    public sealed class EmitFibonaciUnitTests
    {
        [TestMethod]
        public void EmitExecuteFibonacci_Iterative()
        {
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
            type Test
            {
                i32 Method()
                {
                    i32 count = 8;

                    i32 a = 0;
                    i32 b = 1;
                    i32 c = 0;

                    for(i32 i = 2;i<count;)
                    {
                        c = a + b;
                        a = b;
                        b = c;
                        i+= 1;
                    }
                    return c;
                }
            }"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            // Execute code
            __memory.InitStack();
            __interpreter.ExecuteBytecode(methodModel.MethodHandle, stream.ToArray());

            // Get value on top of stack
            Assert.AreEqual(21, __interpreter.FetchValue<int>(16));
        }
    }
}
