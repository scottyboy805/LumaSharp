using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Runtime;
using LumaSharp.Compiler.Emit;

namespace CompilerTests.Emit.Instructions
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
                .WithBody(Syntax.LocalVariable(Syntax.TypeReference("Test"), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), 
                        Syntax.VariableAssignment(Syntax.MemberReference(Syntax.VariableReference("myVar"), "myField"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count > 0);
            Assert.AreEqual(OpCode.Ld_Var_0, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_Fld, builder[1].OpCode);
        }

        [TestMethod]
        public void EmitLoadField_Address()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Field("myField", Syntax.TypeReference("Test")),
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "param1", true))
                .WithBody(Syntax.LocalVariable(Syntax.TypeReference("Test"), "myVar"),
                Syntax.Return(Syntax.MethodInvoke(Syntax.MemberReference(Syntax.MemberReference(Syntax.VariableReference("myVar"), "myField"), "Test"), Syntax.ArgumentList(Syntax.VariableReference("myVar")))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count > 0);
            Assert.AreEqual(OpCode.Ld_Var_0, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_Fld_A, builder[1].OpCode);
        }
    }
}
