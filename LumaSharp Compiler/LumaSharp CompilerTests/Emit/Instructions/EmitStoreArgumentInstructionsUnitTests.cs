using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Emit.Builder;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Runtime;

namespace LumaSharp_CompilerTests.Emit.Instructions
{
    [TestClass]
    public sealed class EmitStoreArgumentInstructionsUnitTests
    {
        [TestMethod]
        public void EmitStoreArg_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myArg"))
                .WithStatements(Syntax.Assign(Syntax.VariableReference("myArg"), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.InstructionIndex > 1);
            Assert.AreEqual(OpCode.St_Arg_1, builder[1].opCode);
        }

        [TestMethod]
        public void EmitStoreArg_2()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a1"),
                    Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myArg"))
                .WithStatements(Syntax.Assign(Syntax.VariableReference("myArg"), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.InstructionIndex > 1);
            Assert.AreEqual(OpCode.St_Arg_2, builder[1].opCode);
        }

        [TestMethod]
        public void EmitStoreArg_3()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a1"),
                    Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a2"),
                    Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myArg"))
                .WithStatements(Syntax.Assign(Syntax.VariableReference("myArg"), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.InstructionIndex > 1);
            Assert.AreEqual(OpCode.St_Arg_3, builder[1].opCode);
        }

        [TestMethod]
        public void EmitStoreArg_Index()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a1"),
                    Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a2"),
                    Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a3"),
                    Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myArg"))
                .WithStatements(Syntax.Assign(Syntax.VariableReference("myArg"), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.InstructionIndex > 1);
            Assert.AreEqual(OpCode.St_Arg, builder[1].opCode);
            Assert.AreEqual((byte)4, builder[1].data0);
        }

        [TestMethod]
        public void EmitStoreArg_Index_Extended()
        {
            ParameterSyntax[] parameters = new ParameterSyntax[byte.MaxValue + 1];

            for (int i = 0; i < byte.MaxValue; i++)
                parameters[i] = Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a" + i);

            // Store our target parameter last
            parameters[byte.MaxValue] = Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myArg");

            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(parameters)
                .WithStatements(Syntax.Assign(Syntax.VariableReference("myArg"), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.InstructionIndex > 1);
            Assert.AreEqual(OpCode.St_Arg_E, builder[1].opCode);
            Assert.AreEqual((ushort)(byte.MaxValue + 1), builder[1].data0);
        }

        [TestMethod]
        public void EmitStoreArg_1_Addr()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myArg", true))
                .WithStatements(Syntax.Assign(Syntax.VariableReference("myArg"), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(new MemoryStream()));
            new MethodBodyBuilder(methodModel.BodyStatements).BuildEmitObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.InstructionIndex > 1);
            Assert.AreEqual(OpCode.Ld_Arg_1, builder[1].opCode);
            Assert.AreEqual(OpCode.St_Addr_I4, builder[2].opCode);
        }
    }
}
