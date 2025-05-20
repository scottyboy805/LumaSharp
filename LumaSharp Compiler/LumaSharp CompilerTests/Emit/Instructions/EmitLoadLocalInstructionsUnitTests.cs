using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Runtime;
using LumaSharp.Compiler.Emit;

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
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"),
                        Syntax.VariableAssignment(Syntax.VariableReference("myVar"))))));

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
        }

        [TestMethod]
        public void EmitLoadLocal_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "l0"),
                    Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"),
                        Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.VariableReference("myVar"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count > 1);
            Assert.AreEqual(OpCode.Ld_Var_1, builder[0].OpCode);
        }

        [TestMethod]
        public void EmitLoadLocal_2()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "l0"),
                Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "l1"),
                    Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"),
                        Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.VariableReference("myVar"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count > 1);
            Assert.AreEqual(OpCode.Ld_Var_2, builder[0].OpCode);
        }

        [TestMethod]
        public void EmitLoadLocal_Index()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "l0"),
                Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "l1"),
                Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "l2"),
                    Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"),
                        Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.VariableReference("myVar"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count > 1);
            Assert.AreEqual(OpCode.Ld_Var, builder[0].OpCode);
            Assert.AreEqual((byte)3, builder[0].Operand);
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
            statements[byte.MaxValue + 1] = Syntax.Assign(Syntax.VariableReference("myVar"),
                Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.VariableReference("myVar")));

            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(statements)));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.DiagnosticCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count > 1);
            Assert.AreEqual(OpCode.Ld_Var_E, builder[0].OpCode);
            Assert.AreEqual((ushort)byte.MaxValue, builder[0].Operand);
        }
    }
}
