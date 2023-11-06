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
    public sealed class EmitLoadFieldInstructionsUnitTests
    {
        [TestMethod]
        public void EmitLoadField()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Field("myField", Syntax.TypeReference("Test")),
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference("Test"), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.FieldReference("myField", Syntax.VariableReference("myVar"))))));

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
            Assert.AreEqual(OpCode.Ld_Loc_0, builder[0].opCode);
            Assert.AreEqual(OpCode.Ld_Fld, builder[1].opCode);
        }

        [TestMethod]
        public void EmitLoadField_Address()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Field("myField", Syntax.TypeReference("Test")),
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "param1", true))
                .WithStatements(Syntax.Variable(Syntax.TypeReference("Test"), "myVar"),
                Syntax.Return(Syntax.MethodInvoke("Test", Syntax.FieldReference("myField", Syntax.VariableReference("myVar")))
                .WithArguments(Syntax.VariableReference("myVar"))))));

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
            Assert.AreEqual(OpCode.Ld_Loc_0, builder[0].opCode);
            Assert.AreEqual(OpCode.Ld_Fld_A, builder[1].opCode);
        }
    }
}
