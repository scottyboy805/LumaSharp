using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime;
using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Emit.Builder;
using LumaSharp_Compiler.Semantics.Model.Statement;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp_Compiler;

namespace LumaSharp_CompilerTests.Emit.IntegratedSource
{
    [TestClass]
    public sealed class EmitExecuteConditionSourceUnitTests
    {
        [TestMethod]
        public void EmitExecuteCondition_Simple_True()
        {
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
            type Test
            {
                i32 Method()
                {
                    if(true) 
                        return 33;
                    return 22;
                }
            }"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            // Execute code
            __memory.InitStack();
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Get value on top of stack
            Assert.AreEqual(33, __interpreter.FetchValue<int>());
        }

        [TestMethod]
        public void EmitExecuteCondition_Simple_False()
        {
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
            type Test
            {
                i32 Method()
                {
                    if(false) 
                        return 33;
                    return 22;
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
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Get value on top of stack
            Assert.AreEqual(22, __interpreter.FetchValue<int>());
        }

        [TestMethod]
        public void EmitExecuteCondition_SimpleElse_True()
        {
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
            type Test
            {
                i32 Method()
                {
                    if(true) 
                        return 33;
                    else
                        return 22;
                }
            }"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            // Execute code
            __memory.InitStack();
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Get value on top of stack
            Assert.AreEqual(33, __interpreter.FetchValue<int>());
        }

        [TestMethod]
        public void EmitExecuteCondition_SimpleElse_False()
        {
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
            type Test
            {
                i32 Method()
                {
                    if(false) 
                        return 33;
                    else
                        return 22;
                }
            }"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            // Execute code
            __memory.InitStack();
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Get value on top of stack
            Assert.AreEqual(22, __interpreter.FetchValue<int>());
        }

        [TestMethod]
        public void EmitExecuteCondition_SimpleAlternate_True()
        {
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
            type Test
            {
                i32 Method()
                {
                    if(true) 
                        return 33;
                    else if(false)
                        return 22;
                }
            }"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            // Execute code
            __memory.InitStack();
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Get value on top of stack
            Assert.AreEqual(33, __interpreter.FetchValue<int>());
        }

        [TestMethod]
        public void EmitExecuteCondition_SimpleAlternate_False()
        {
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
            type Test
            {
                i32 Method()
                {
                    if(false) 
                        return 33;
                    else if(true)
                        return 22;
                }
            }"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            // Execute code
            __memory.InitStack();
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Get value on top of stack
            Assert.AreEqual(22, __interpreter.FetchValue<int>());
        }

        [TestMethod]
        public void EmitExecuteCondition_SimpleAlternateElse_1()
        {
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
            type Test
            {
                i32 Method()
                {
                    if(1) 
                        return 33;
                    else if(2)
                        return 22;
                    else
                        return 11;
                }
            }"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            // Execute code
            __memory.InitStack();
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Get value on top of stack
            Assert.AreEqual(33, __interpreter.FetchValue<int>());
        }

        [TestMethod]
        public void EmitExecuteCondition_SimpleAlternateElse_2()
        {
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(@"
            type Test
            {
                i32 Method()
                {
                    if(2) 
                        return 33;
                    else if(1)
                        return 22;
                    else
                        return 11;
                }
            }"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            // Execute code
            __memory.InitStack();
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Get value on top of stack
            Assert.AreEqual(22, __interpreter.FetchValue<int>());
        }
    }
}
