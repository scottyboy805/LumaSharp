//using LumaSharp.Runtime.Emit;
//using LumaSharp.Runtime;
//using LumaSharp_Compiler.AST;
//using LumaSharp_Compiler.Emit.Builder;
//using LumaSharp_Compiler.Semantics.Model.Statement;
//using LumaSharp_Compiler.Semantics.Model;
//using LumaSharp_Compiler;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace LumaSharp_CompilerTests.Emit.IntegratedSource
//{
//    [TestClass]
//    public sealed class EmitExecuteForSourceUnitTests
//    {
//        [TestMethod]
//        public void EmitExecuteFor_Simple_True()
//        {
//            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
//            type Test
//            {
//                i32 Method()
//                {
//                    for(;1;)
//                    {
//                        return 33;
//                    }
//                    return 22;
//                }
//            }"));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(methodModel);
//            Assert.AreEqual(0, model.Report.MessageCount);

//            // Build instructions
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
//            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

//            // Execute code
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Get value on top of stack
//            Assert.AreEqual(33, __interpreter.FetchValue<int>());
//        }

//        [TestMethod]
//        public void EmitExecuteFor_Simple_False()
//        {
//            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
//            type Test
//            {
//                i32 Method()
//                {
//                    for(;0;)
//                    {
//                        return 33;
//                    }
//                    return 22;
//                }
//            }"));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(methodModel);
//            Assert.AreEqual(0, model.Report.MessageCount);

//            // Build instructions
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
//            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

//            // Execute code
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Get value on top of stack
//            Assert.AreEqual(22, __interpreter.FetchValue<int>());
//        }

//        [TestMethod]
//        public void EmitExecuteFor_Count_Variable()
//        {
//            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
//            type Test
//            {
//                i32 Method()
//                {
//                    for(i32 i=0;i < 3;)
//                    {
//                        return 33;
//                    }
//                    return 22;
//                }
//            }"));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(methodModel);
//            Assert.AreEqual(0, model.Report.MessageCount);

//            // Build instructions
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
//            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

//            // Execute code
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(methodModel.MethodHandle, stream.ToArray());

//            // Get value on top of stack
//            Assert.AreEqual(33, __interpreter.FetchValue<int>(8));
//        }

//        [TestMethod]
//        public void EmitExecuteFor_CountValue_Variable()
//        {
//            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
//            type Test
//            {
//                i32 Method()
//                {
//                    i32 val = 0;
//                    for(i32 i=0;i < 3;)
//                    {
//                        val = val + 1;
//                        i = i + 1;
//                    }
//                    return val;
//                }
//            }"));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(methodModel);
//            Assert.AreEqual(0, model.Report.MessageCount);

//            // Build instructions
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
//            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

//            // Execute code
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(methodModel.MethodHandle, stream.ToArray());

//            // Get value on top of stack
//            Assert.AreEqual(3, __interpreter.FetchValue<int>(8));
//        }

//        [TestMethod]
//        public void EmitExecuteFor_CountValue_VariableIncrement()
//        {
//            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
//            type Test
//            {
//                i32 Method()
//                {
//                    i32 val = 5;
//                    for(i32 i=0;i < 10;)
//                    {
//                        val = val + 1;
//                        i+=1;
//                    }
//                    return val;
//                }
//            }"));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(methodModel);
//            Assert.AreEqual(0, model.Report.MessageCount);

//            // Build instructions
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
//            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

//            // Execute code
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(methodModel.MethodHandle, stream.ToArray());

//            // Get value on top of stack
//            Assert.AreEqual(15, __interpreter.FetchValue<int>(8));
//        }
//    }
//}
