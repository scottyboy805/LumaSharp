using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Runtime;
using LumaSharp.Compiler.Emit;

namespace CompilerTests.Emit.Instructions
{
    [TestClass]
    public sealed class EmitStoreLocalInstructionsUnitTests
    {
        [TestMethod]
        public void EmitStoreLocal_0()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.LocalVariable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"),
                        Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.Literal(5))))));

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
            Assert.AreEqual(OpCode.St_Var_0, builder[1].OpCode);
        }

        [TestMethod]
        public void EmitStoreLocal_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.LocalVariable(Syntax.TypeReference(PrimitiveType.Any), "l0"),
                    Syntax.LocalVariable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"),
                        Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.Literal(5))))));

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
            Assert.AreEqual(OpCode.St_Var_1, builder[1].OpCode);
        }

        [TestMethod]
        public void EmitStoreLocal_2()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.LocalVariable(Syntax.TypeReference(PrimitiveType.Any), "l0"),
                Syntax.LocalVariable(Syntax.TypeReference(PrimitiveType.Any), "l1"),
                    Syntax.LocalVariable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"),
                        Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.Literal(5))))));

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
            Assert.AreEqual(OpCode.St_Var_2, builder[1].OpCode);
        }

        [TestMethod]
        public void EmitStoreLocal_Index()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.LocalVariable(Syntax.TypeReference(PrimitiveType.Any), "l0"),
                Syntax.LocalVariable(Syntax.TypeReference(PrimitiveType.Any), "l1"),
                Syntax.LocalVariable(Syntax.TypeReference(PrimitiveType.Any), "l2"),
                    Syntax.LocalVariable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"),
                        Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.Literal(5))))));

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
            Assert.AreEqual(OpCode.St_Var, builder[1].OpCode);
            Assert.AreEqual((byte)3, builder[1].Operand);
        }

        [TestMethod]
        public void EmitStoreLocal_Index_Extended()
        {
            // If the local index is greater than 255, the load local index will be ushort instead of byte using the extended instruction
            StatementSyntax[] statements = new StatementSyntax[byte.MaxValue + 2];

            for (int i = 0; i < byte.MaxValue; i++)
                statements[i] = Syntax.LocalVariable(Syntax.TypeReference(PrimitiveType.I32), "l" + i);

            // Store our variable last
            statements[byte.MaxValue] = Syntax.LocalVariable(Syntax.TypeReference(PrimitiveType.Any), "myVar");

            // Store our access expression finally
            statements[byte.MaxValue + 1] = Syntax.Assign(Syntax.VariableReference("myVar"),
                Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.Literal(5)));

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
            Assert.AreEqual(OpCode.St_Var_E, builder[1].OpCode);
            Assert.AreEqual((ushort)byte.MaxValue, builder[1].Operand);
        }
    }
}
