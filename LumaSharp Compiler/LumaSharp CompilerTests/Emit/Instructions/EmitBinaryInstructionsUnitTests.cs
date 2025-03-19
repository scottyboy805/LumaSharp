using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Runtime;
using LumaSharp.Compiler.Emit;

namespace LumaSharp_CompilerTests.Emit.Instructions
{
    [TestClass]
    public sealed class EmitBinaryInstructionsUnitTests
    {
        [TestMethod]
        public void EmitBinary_Equal_I32_I32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), AssignOperation.Assign, Syntax.Binary(Syntax.Literal(5), BinaryOperation.Equal, Syntax.Literal(2))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 3);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Cmp_Eq, builder[2].OpCode);
        }



        [TestMethod]
        public void EmitBinary_NotEqual_I32_I32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), AssignOperation.Assign, Syntax.Binary(Syntax.Literal(5), BinaryOperation.NotEqual, Syntax.Literal(2))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 3);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Cmp_NEq, builder[2].OpCode);
        }



        [TestMethod]
        public void EmitBinary_Add_I32_I32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), AssignOperation.Assign, Syntax.Binary(Syntax.Literal(5), BinaryOperation.Add, Syntax.Literal(2))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 3);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Add, builder[2].OpCode);
        }

        [TestMethod]
        public void EmitBinary_Add_I32_I64()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), AssignOperation.Assign, Syntax.Binary(Syntax.Literal(5), BinaryOperation.Add, Syntax.Literal(2L))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 3);            
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Cast_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Ld_I8, builder[2].OpCode);
            Assert.AreEqual((byte)PrimitiveType.I64, builder[1].Operand);
            Assert.AreEqual(OpCode.Add, builder[3].OpCode);
        }

        [TestMethod]
        public void EmitBinary_Add_I32_U64()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), AssignOperation.Assign, Syntax.Binary(Syntax.Literal(5), BinaryOperation.Add, Syntax.Literal(2UL))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 3);
            Assert.AreEqual(OpCode.Ld_I8, builder[0].OpCode);
            Assert.AreEqual(OpCode.Cast_I8, builder[1].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[2].OpCode);
            Assert.AreEqual(OpCode.Cast_I4, builder[3].OpCode);
            Assert.AreEqual((byte)PrimitiveType.U64, builder[3].Operand);
            Assert.AreEqual(OpCode.Add, builder[4].OpCode);
        }



        [TestMethod]
        public void EmitBinary_Sub_I32_I32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), AssignOperation.Assign, Syntax.Binary(Syntax.Literal(5), BinaryOperation.Subtract, Syntax.Literal(2))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 3);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Sub, builder[2].OpCode);
        }



        [TestMethod]
        public void EmitBinary_Mul_I32_I32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), AssignOperation.Assign, Syntax.Binary(Syntax.Literal(5), BinaryOperation.Multiply, Syntax.Literal(2))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 3);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Mul, builder[2].OpCode);
        }



        [TestMethod]
        public void EmitBinary_Div_I32_I32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), AssignOperation.Assign, Syntax.Binary(Syntax.Literal(5), BinaryOperation.Divide, Syntax.Literal(2))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 3);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Div, builder[2].OpCode);
        }



        [TestMethod]
        public void EmitBinary_Greater_I32_I32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), AssignOperation.Assign, Syntax.Binary(Syntax.Literal(5), BinaryOperation.Greater, Syntax.Literal(2))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 3);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Cmp_G, builder[2].OpCode);
        }



        [TestMethod]
        public void EmitBinary_GreaterEqual_I32_I32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), AssignOperation.Assign, Syntax.Binary(Syntax.Literal(5), BinaryOperation.GreaterEqual, Syntax.Literal(2))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 3);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Cmp_Ge, builder[2].OpCode);
        }



        [TestMethod]
        public void EmitBinary_Less_I32_I32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), AssignOperation.Assign, Syntax.Binary(Syntax.Literal(5), BinaryOperation.Less, Syntax.Literal(2))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 3);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Cmp_L, builder[2].OpCode);
        }



        [TestMethod]
        public void EmitBinary_LessEqual_I32_I32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "myVar"),
                    Syntax.Assign(Syntax.VariableReference("myVar"), AssignOperation.Assign, Syntax.Binary(Syntax.Literal(5), BinaryOperation.LessEqual, Syntax.Literal(2))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodModel methodModel = model.DescendantsOfType<MethodModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            BytecodeBuilder builder = new BytecodeBuilder();
            new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.BodyStatements).EmitExecutionObject(builder);

            Assert.IsTrue(builder.Count >= 3);
            Assert.AreEqual(OpCode.Ld_I4, builder[0].OpCode);
            Assert.AreEqual(OpCode.Ld_I4, builder[1].OpCode);
            Assert.AreEqual(OpCode.Cmp_Le, builder[2].OpCode);
        }
    }
}
