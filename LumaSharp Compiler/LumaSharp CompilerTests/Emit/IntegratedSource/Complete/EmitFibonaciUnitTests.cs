using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Emit.Builder;
using LumaSharp_Compiler.Semantics.Model;
using LumaSharp_Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppContext = LumaSharp.Runtime.AppContext;
using LumaSharp.Runtime.Reflection;
using System.Diagnostics;

namespace LumaSharp_CompilerTests.Emit.IntegratedSource.Complete
{
    [TestClass]
    public sealed class EmitFibonacciUnitTests
    {
        [TestMethod]
        public void EmitExecuteFibonacci_Iterative()
        {
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
            type Test
            {
                i32 Method()
                {
                    i32 count = 20;

                    i32 a = 0;
                    i32 b = 1;
                    i32 c = 0;

                    for(i32 i = 2;i<=count;)
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

            //Build instructions
            //MemoryStream stream = new MemoryStream();
            //AppContext context = new AppContext();
            //InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
            //new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);
            //new MethodBuilder(context, methodModel)
            //    .EmitExecutableModel(new BinaryWriter(stream));


            //Method method = new Method(context);
            //stream.Seek(0, SeekOrigin.Begin);
            //method.LoadMethodExecutable(new BinaryReader(stream));

            // Emit the method
            Method method = EmitUtil.GetExecutableMethodOnly(methodModel);

            Stopwatch timer = Stopwatch.StartNew();
            // Execute the method
            int result = method.Invoke<int>(null);

            Console.WriteLine("Method invoke took: " + timer.ElapsedMilliseconds + "ms");
            //builder.ToDebugFile("fib.instructions");
            //builder.DebugCheckJumpLocations();

            // Execute code
            //nint r = __interpreter.ExecuteBytecode(methodModel.MethodHandle, stream.ToArray());

            //// Get value on top of stack
            //unsafe
            //{
            //    Assert.AreEqual(21, __interpreter.FetchValue<int>((byte*)r, -4));
            //}
            Assert.AreEqual(6765, result);
        }
    }
}
