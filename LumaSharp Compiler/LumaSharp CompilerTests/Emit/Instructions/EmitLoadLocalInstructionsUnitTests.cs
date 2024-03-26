using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Emit.Builder;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Runtime;
using LumaSharp.Runtime.Emit;

namespace LumaSharp_CompilerTests.Emit.Instructions
{
    [TestClass]
    public sealed class EmitLoadLocalInstructionsUnitTests
    {
        [TestMethod]
        public void EmitLoadLocal_0()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.VariableReference("myVar")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.InstructionIndex > 0);
            Assert.AreEqual(OpCode.Ld_Loc_0, builder[0].opCode);
        }

        [TestMethod]
        public void EmitLoadLocal_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "l0"),
                    Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.VariableReference("myVar")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.InstructionIndex > 1);
            Assert.AreEqual(OpCode.Ld_Loc_1, builder[0].opCode);
        }

        [TestMethod]
        public void EmitLoadLocal_2()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "l0"),
                Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "l1"),
                    Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.VariableReference("myVar")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.InstructionIndex > 1);
            Assert.AreEqual(OpCode.Ld_Loc_2, builder[0].opCode);
        }

        [TestMethod]
        public void EmitLoadLocal_Index()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "l0"),
                Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "l1"),
                Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "l2"),
                    Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.VariableReference("myVar")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.InstructionIndex > 1);
            Assert.AreEqual(OpCode.Ld_Loc, builder[0].opCode);
            Assert.AreEqual((byte)3, builder[0].data0);
        }

        [TestMethod]
        public void EmitLoadLocal_Index_Extended()
        {
            // If the local index is greater than 255, the load local index will be ushort instead of byte using the extended instruction
            StatementSyntax[] statements = new StatementSyntax[byte.MaxValue + 2];

            for (int i = 0; i < byte.MaxValue; i++)
                statements[i] = Syntax.Variable(Syntax.TypeReference(PrimitiveType.I32), "l" + i);

            // Store our variable last
            statements[byte.MaxValue] = Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar");

            // Store our access expression finally
            statements[byte.MaxValue + 1] = Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.VariableReference("myVar"));

            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(statements)));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.InstructionIndex > 1);
            Assert.AreEqual(OpCode.Ld_Loc_E, builder[0].opCode);
            Assert.AreEqual((ushort)byte.MaxValue, builder[0].data0);
        }
    }
}
