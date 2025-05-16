using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Runtime;
using LumaSharp.Runtime.Emit;
using LumaSharp.Compiler.Emit;

namespace LumaSharp_CompilerTests.Emit.Instructions
{
    [TestClass]
    public sealed class EmitLoadArgumentInstructionsUnitTests
    {
        [TestMethod]
        public void EmitLoadArg_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myArg"))
                .WithBody(Syntax.Assign(Syntax.VariableReference("myArg"),
                    Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.VariableReference("myArg"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeGenerator generator = new BytecodeGenerator();
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.Count > 1);
            Assert.AreEqual(OpCode.Ld_Var_1, builder[0].OpCode);
        }

        [TestMethod]
        public void EmitLoadArg_2()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a1"),
                    Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myArg"))
                .WithBody(Syntax.Assign(Syntax.VariableReference("myArg"),
                    Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.VariableReference("myArg"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.Count > 1);
            Assert.AreEqual(OpCode.Ld_Var_2, builder[0].OpCode);
        }

        [TestMethod]
        public void EmitLoadArg_3()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a1"),
                    Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a2"),
                    Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myArg"))
                .WithBody(Syntax.Assign(Syntax.VariableReference("myArg"),
                    Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.VariableReference("myArg"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.Count > 1);
            Assert.AreEqual(OpCode.Ld_Var_3, builder[0].OpCode);
        }

        [TestMethod]
        public void EmitLoadArg_Index()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a1"),
                    Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a2"),
                    Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a3"),
                    Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myArg"))
                .WithBody(Syntax.Assign(Syntax.VariableReference("myArg"),
                    Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.VariableReference("myArg"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.Count > 1);
            Assert.AreEqual(OpCode.Ld_Var, builder[0].OpCode);
            Assert.AreEqual((byte)4, builder[0].Operand);
        }

        [TestMethod]
        public void EmitLoadArg_Index_Extended()
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
                .WithBody(Syntax.Assign(Syntax.VariableReference("myArg"),
                    Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.VariableReference("myArg"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.Count > 1);
            Assert.AreEqual(OpCode.Ld_Var_E, builder[0].OpCode);
            Assert.AreEqual((ushort)(byte.MaxValue + 1), builder[0].Operand);
        }

        [TestMethod]
        public void EmitLoadArg_1_Addr()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myArg", true))
                .WithBody(Syntax.Assign(Syntax.VariableReference("myArg"),
                    Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.VariableReference("myArg"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.Count > 1);
            Assert.AreEqual(OpCode.Ld_Var_1, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_Addr, builder[1].OpCode);
        }

        [TestMethod]
        public void EmitLoadArg_1_Addr_Extended()
        {
            ParameterSyntax[] parameters = new ParameterSyntax[byte.MaxValue + 1];

            for (int i = 0; i < byte.MaxValue; i++)
                parameters[i] = Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a" + i);

            // Store our target parameter last
            parameters[byte.MaxValue] = Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myArg", true);

            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(parameters)
                .WithBody(Syntax.Assign(Syntax.VariableReference("myArg"),
                    Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.VariableReference("myArg"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            // Note that for instance methods arg0 is reserved for `this` instance
            Assert.IsTrue(builder.Count > 1);
            Assert.AreEqual(OpCode.Ld_Var_E, builder[0].OpCode);
            Assert.AreEqual((ushort)(byte.MaxValue + 1), builder[0].Operand);
            Assert.AreEqual(OpCode.Ld_Addr, builder[1].OpCode);
        }
    }
}
