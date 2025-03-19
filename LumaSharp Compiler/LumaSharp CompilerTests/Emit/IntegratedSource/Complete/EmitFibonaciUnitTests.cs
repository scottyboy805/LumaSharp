using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using LumaSharp.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            // Emit the method
            MetaMethod method = EmitUtil.GetExecutableMethodOnly(methodModel);

            Stopwatch timer = Stopwatch.StartNew();
            // Execute the method
            int result = method.Invoke<int>(null);

            Console.WriteLine("Method invoke took: " + timer.Elapsed.TotalMilliseconds + "ms");            
            Assert.AreEqual(6765, result);
        }

        [TestMethod]
        public void EmitExecuteFibonacci_Recursive()
        {
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
            type Test
            {
                i32 Method(i32 n)
                {
                    if(n < 2)
                        return n;
                    else
                        return Method(n - 1) + Method(n - 2);        
                }
            }"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Emit the method
            MetaMethod method = EmitUtil.GetExecutableMethodOnly(methodModel);

            Stopwatch timer = Stopwatch.StartNew();
            // Execute the method
            int result = method.Invoke<int>(new object[] { 2 });

            Console.WriteLine("Method invoke took: " + timer.Elapsed.TotalMilliseconds + "ms");
            Assert.AreEqual(6765, result);
        }
    }
}
