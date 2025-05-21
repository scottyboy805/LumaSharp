//using LumaSharp_Compiler.AST.Factory;
//using LumaSharp_Compiler.AST;
//using LumaSharp_Compiler.Emit.Builder;
//using LumaSharp_Compiler.Semantics.Model.Statement;
//using LumaSharp_Compiler.Semantics.Model;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using LumaSharp.Runtime;
//using LumaSharp.Runtime.Emit;

//namespace CompilerTests.Emit.Integrated
//{
//    [TestClass]
//    public unsafe sealed class EmitExecuteConditionSyntaxUnitTests
//    {
//        [TestMethod]
//        public void EmitExecuteCondition_Simple_True()
//        {
//            SyntaxTree tree = SyntaxTree.Create(
//                Syntax.Type("Test").WithMembers(
//                Syntax.Method("Test", Syntax.TypeReference(PrimitiveType.I32))
//                .WithStatements(Syntax.Condition(Syntax.Literal(true))
//                .WithInlineStatement(Syntax.Return(Syntax.Literal(5))))));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(conditionModel);
//            Assert.AreEqual(0, model.Report.MessageCount);

//            // Build instructions
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
//            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

//            // Execute code
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Get value on top of stack
//            Assert.AreEqual(5, __interpreter.FetchValue<int>());
//        }

//        [TestMethod]
//        public void EmitExecuteCondition_Simple_False()
//        {
//            SyntaxTree tree = SyntaxTree.Create(
//                Syntax.Type("Test").WithMembers(
//                Syntax.Method("Test", Syntax.TypeReference(PrimitiveType.I32))
//                .WithStatements(Syntax.Condition(Syntax.Literal(false))
//                .WithInlineStatement(Syntax.Return(Syntax.Literal(5))))));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(conditionModel);
//            Assert.AreEqual(0, model.Report.MessageCount);

//            // Build instructions
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
//            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

//            // Execute code
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Get value on top of stack
//            Assert.AreEqual(0, __interpreter.FetchValue<int>());
//        }

//        [TestMethod]
//        public void EmitExecuteCondition_Simple_Greater()
//        {
//            SyntaxTree tree = SyntaxTree.Create(
//                Syntax.Type("Test").WithMembers(
//                Syntax.Method("Test", Syntax.TypeReference(PrimitiveType.I32))
//                .WithStatements(Syntax.Condition(Syntax.Binary(Syntax.Literal(2), BinaryOperation.Greater, Syntax.Literal(1)))
//                .WithInlineStatement(Syntax.Return(Syntax.Literal(33))))));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(conditionModel);
//            Assert.AreEqual(0, model.Report.MessageCount);

//            // Build instructions
//            MemoryStream stream = new MemoryStream();
//            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
//            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

//            // Execute code
//            __memory.InitStack();
//            __interpreter.ExecuteBytecode(stream.ToArray());

//            // Get value on top of stack
//            Assert.AreEqual(33, __interpreter.FetchValue<int>());
//        }
//    }
//}
