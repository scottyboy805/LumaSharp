using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Runtime;
using LumaSharp.Compiler.Emit;

namespace CompilerTests.Emit.Instructions
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
                .WithBody(Syntax.Condition(Syntax.Literal(true)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(0, new StatementModel[] { conditionModel }).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 2);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].OpCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].OpCode);
            Assert.AreEqual(2, builder[1].Operand);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_False()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Condition(Syntax.Literal(false)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(0, new StatementModel[] { conditionModel }).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 2);
            Assert.AreEqual(OpCode.Ld_I4_0, builder[0].OpCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].OpCode);
            Assert.AreEqual(2, builder[1].Operand);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Condition(Syntax.Literal(1)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(0, new StatementModel[] { conditionModel }).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 2);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].OpCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].OpCode);
            Assert.AreEqual(2, builder[1].Operand);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_CompareEqual()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Condition(Syntax.Binary(Syntax.Literal(2), Syntax.Token(SyntaxTokenKind.EqualitySymbol), Syntax.Literal(4))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(0, new StatementModel[] { conditionModel }).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 4);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Cmp_Eq, builder[2].OpCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[3].OpCode);
            Assert.AreEqual(4, builder[3].Operand);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_CompareNotEqual()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Condition(Syntax.Binary(Syntax.Literal(2), Syntax.Token(SyntaxTokenKind.NonEqualitySymbol), Syntax.Literal(4))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(0, new StatementModel[] { conditionModel }).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 4);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Cmp_NEq, builder[2].OpCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[3].OpCode);
            Assert.AreEqual(4, builder[3].Operand);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_Else_True()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Condition(Syntax.Literal(true)).WithInlineStatement(Syntax.Return())
                .WithAlternate(Syntax.AlternateCondition().WithInlineStatement(Syntax.Return())))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(0, new StatementModel[] { conditionModel }).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 2);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].OpCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].OpCode);
            Assert.AreEqual(OpCode.Ret, builder[2].OpCode);
            Assert.AreEqual(OpCode.Ret, builder[3].OpCode);
            Assert.AreEqual(4, builder[1].Operand);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_Elif_True()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Condition(Syntax.Literal(true)).WithInlineStatement(Syntax.Return())
                .WithAlternate(Syntax.Condition(Syntax.Literal(false)).WithInlineStatement(Syntax.Return())))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(0, new StatementModel[] { conditionModel }).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 2);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].OpCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].OpCode);
            Assert.AreEqual(OpCode.Ret, builder[2].OpCode);
            Assert.AreEqual(OpCode.Ld_I4_0, builder[3].OpCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[4].OpCode);
            Assert.AreEqual(OpCode.Ret, builder[5].OpCode);
            Assert.AreEqual(3, builder[1].Operand);   // Jmp index
            Assert.AreEqual(6, builder[4].Operand);   // Jmp index
        }

        [TestMethod]
        public void EmitCondition_Elif_Else_True()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Condition(Syntax.Literal(true)).WithInlineStatement(Syntax.Return())
                .WithAlternate(Syntax.Condition(Syntax.Literal(false)).WithInlineStatement(Syntax.Return())
                .WithAlternate(Syntax.AlternateCondition().WithInlineStatement(Syntax.Return()))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(0, new StatementModel[] { conditionModel }).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 2);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].OpCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].OpCode);
            Assert.AreEqual(OpCode.Ret, builder[2].OpCode);
            Assert.AreEqual(OpCode.Ld_I4_0, builder[3].OpCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[4].OpCode);
            Assert.AreEqual(OpCode.Ret, builder[5].OpCode);
            Assert.AreEqual(OpCode.Ret, builder[6].OpCode);
            Assert.AreEqual(3, builder[1].Operand);   // Jmp index
            Assert.AreEqual(7, builder[4].Operand);   // Jmp index
        }
    }
}
