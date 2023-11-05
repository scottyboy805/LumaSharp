using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Emit.Builder;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Runtime;
using LumaSharp_Compiler.Semantics.Model.Statement;

namespace LumaSharp_CompilerTests.Emit.Instructions
{
    [TestClass]
    public sealed class EmitConditionInstructionsUnitTests
    {
        [TestMethod]
        public void EmitCondition_True()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Condition(Syntax.Literal(true)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex >= 2);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].opCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].opCode);
            Assert.AreEqual(2, builder[1].data0);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_False()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Condition(Syntax.Literal(false)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex >= 2);
            Assert.AreEqual(OpCode.Ld_I4_0, builder[0].opCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].opCode);
            Assert.AreEqual(2, builder[1].data0);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Condition(Syntax.Literal(1)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex >= 2);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].opCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].opCode);
            Assert.AreEqual(2, builder[1].data0);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_CompareEqual()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Condition(Syntax.Binary(Syntax.Literal(2), BinaryOperation.Equal, Syntax.Literal(4))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex >= 4);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].opCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].opCode);
            Assert.AreEqual(OpCode.Cmp_Eq, builder[2].opCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[3].opCode);
            Assert.AreEqual(4, builder[3].data0);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_CompareNotEqual()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Condition(Syntax.Binary(Syntax.Literal(2), BinaryOperation.NotEqual, Syntax.Literal(4))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex >= 4);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].opCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].opCode);
            Assert.AreEqual(OpCode.Cmp_NEq, builder[2].opCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[3].opCode);
            Assert.AreEqual(4, builder[3].data0);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_Else_True()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Condition(Syntax.Literal(true)).WithInlineStatement(Syntax.Return())
                .WithAlternate(Syntax.Condition().WithInlineStatement(Syntax.Return())))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex >= 2);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].opCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].opCode);
            Assert.AreEqual(OpCode.Ret, builder[2].opCode);
            Assert.AreEqual(OpCode.Ret, builder[3].opCode);
            Assert.AreEqual(4, builder[1].data0);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_Elif_True()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Condition(Syntax.Literal(true)).WithInlineStatement(Syntax.Return())
                .WithAlternate(Syntax.Condition(Syntax.Literal(false)).WithInlineStatement(Syntax.Return())))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex >= 2);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].opCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].opCode);
            Assert.AreEqual(OpCode.Ret, builder[2].opCode);
            Assert.AreEqual(OpCode.Ld_I4_0, builder[3].opCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[4].opCode);
            Assert.AreEqual(OpCode.Ret, builder[5].opCode);
            Assert.AreEqual(3, builder[1].data0);   // Jmp index
            Assert.AreEqual(6, builder[4].data0);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_Elif_Else_True()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Condition(Syntax.Literal(true)).WithInlineStatement(Syntax.Return())
                .WithAlternate(Syntax.Condition(Syntax.Literal(false)).WithInlineStatement(Syntax.Return())
                .WithAlternate(Syntax.Condition().WithInlineStatement(Syntax.Return()))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex >= 2);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].opCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].opCode);
            Assert.AreEqual(OpCode.Ret, builder[2].opCode);
            Assert.AreEqual(OpCode.Ld_I4_0, builder[3].opCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[4].opCode);
            Assert.AreEqual(OpCode.Ret, builder[5].opCode);
            Assert.AreEqual(OpCode.Ret, builder[6].opCode);
            Assert.AreEqual(3, builder[1].data0);   // Jmp index
            Assert.AreEqual(7, builder[4].data0);   // Jmp index
        }
    }
}
