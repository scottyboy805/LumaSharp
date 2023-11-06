using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Semantics.Model.Expression;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp_Compiler.Emit.Builder;
using LumaSharp.Runtime;
using LumaSharp.Runtime.Emit;

namespace LumaSharp_CompilerTests.Emit.Instructions
{
    [TestClass]
    public sealed class EmitConstantInstructionsUnitTests
    {
        [TestMethod]
        public void EmitConstant_Null()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.LiteralNull()))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex > 0);
            Assert.AreEqual(OpCode.Ld_Null, builder[0].opCode);
        }

        [TestMethod]
        public void EmitConstant_True()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Bool), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.Literal(true)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex > 0);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].opCode);
        }

        [TestMethod]
        public void EmitConstant_False()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Bool), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.Literal(false)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex > 0);
            Assert.AreEqual(OpCode.Ld_I4_0, builder[0].opCode);
        }

        [TestMethod]
        public void EmitConstant_I32_M1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.I32), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.Literal(-1)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            // Should use optimized instruction
            Assert.IsTrue(builder.InstructionIndex > 0);
            Assert.AreEqual(OpCode.Ld_I4_M1, builder[0].opCode);
        }

        [TestMethod]
        public void EmitConstant_I32_0()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.I32), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.Literal(0)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            // Should use optimized instruction
            Assert.IsTrue(builder.InstructionIndex > 0);
            Assert.AreEqual(OpCode.Ld_I4_0, builder[0].opCode);
        }

        [TestMethod]
        public void EmitConstant_I32_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.I32), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.Literal(1)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            // Should use optimized instruction
            Assert.IsTrue(builder.InstructionIndex > 0);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].opCode);
        }

        [TestMethod]
        public void EmitConstant_I32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.I32), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex > 0);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].opCode);
            Assert.AreEqual(5, builder[0].data0);
        }

        [TestMethod]
        public void EmitConstant_U32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.U32), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.Literal(5U)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex > 0);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].opCode);
            Assert.AreEqual((uint)5, builder[0].data0);
        }

        [TestMethod]
        public void EmitConstant_I64()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.I64), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.Literal(5L)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex > 0);
            Assert.AreEqual(OpCode.Ld_I8, builder[0].opCode);
            Assert.AreEqual(5L, builder[0].data0);
        }

        [TestMethod]
        public void EmitConstant_U64()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.U64), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.Literal(5UL)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex > 0);
            Assert.AreEqual(OpCode.Ld_I8, builder[0].opCode);
            Assert.AreEqual(5UL, builder[0].data0);
        }

        [TestMethod]
        public void EmitConstant_String()
        {
            throw new NotImplementedException("To be completed");
        }
    }
}
