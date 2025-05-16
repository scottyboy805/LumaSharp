using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.Reporting
{
    [TestClass]
    public sealed class ReportSpecialOperatorMessagesUnitTests
    {
        [TestMethod]
        public void ReportSpecialOperatorMessages_Equals_NonGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_equal", Syntax.TypeReference(PrimitiveType.Bool))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorMustBeGlobal, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Equals_InvalidReturn()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_equal", Syntax.TypeReference(PrimitiveType.I32))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectReturn, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Equals_InvalidParameterCount()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_equal", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameterCount, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Equals_InvalidParameter_0()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_equal", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameter, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Equals_InvalidParameter_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_equal", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference(PrimitiveType.Bool), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameter, model.Report.Messages.First().Code);
        }


        [TestMethod]
        public void ReportSpecialOperatorMessages_Hash_NonGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_hash", Syntax.TypeReference(PrimitiveType.I32))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorMustBeGlobal, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Hash_InvalidReturn()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_hash", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectReturn, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Hash_InvalidParameters()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_hash", Syntax.TypeReference(PrimitiveType.I32))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "par"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectVoidParameter, model.Report.Messages.First().Code);
        }



        [TestMethod]
        public void ReportSpecialOperatorMessages_String_NonGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_string", Syntax.TypeReference("string"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorMustBeGlobal, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_String_InvalidReturn()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_string", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectReturn, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_String_InvalidParameters()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_string", Syntax.TypeReference("string"))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "par"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectVoidParameter, model.Report.Messages.First().Code);
        }



        [TestMethod]
        public void ReportSpecialOperatorMessages_Add_NonGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_add", Syntax.TypeReference(PrimitiveType.Bool))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorMustBeGlobal, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Add_InvalidParameterCount()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_add", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameterCount, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Add_InvalidParameter()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_add", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameter, model.Report.Messages.First().Code);
        }



        [TestMethod]
        public void ReportSpecialOperatorMessages_Subtract_NonGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_subtract", Syntax.TypeReference(PrimitiveType.Bool))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorMustBeGlobal, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Subtract_InvalidParameterCount()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_subtract", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameterCount, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Subtract_InvalidParameter()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_subtract", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameter, model.Report.Messages.First().Code);
        }



        [TestMethod]
        public void ReportSpecialOperatorMessages_Multiply_NonGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_multiply", Syntax.TypeReference(PrimitiveType.Bool))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorMustBeGlobal, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Multiply_InvalidParameterCount()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_multiply", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameterCount, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Multiply_InvalidParameter()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_multiply", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameter, model.Report.Messages.First().Code);
        }



        [TestMethod]
        public void ReportSpecialOperatorMessages_Divide_NonGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_divide", Syntax.TypeReference(PrimitiveType.Bool))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorMustBeGlobal, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Divide_InvalidParameterCount()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_divide", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameterCount, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Divide_InvalidParameter()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_divide", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameter, model.Report.Messages.First().Code);
        }



        [TestMethod]
        public void ReportSpecialOperatorMessages_Negate_NonGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_negate", Syntax.TypeReference("Test"))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorMustBeGlobal, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Negate_InvalidParameterCount()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_negate", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "par"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameterCount, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Negate_InvalidParameter()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_negate", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameter, model.Report.Messages.First().Code);
        }



        [TestMethod]
        public void ReportSpecialOperatorMessages_Greater_NonGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_greater", Syntax.TypeReference(PrimitiveType.Bool))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorMustBeGlobal, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Greater_InvalidReturn()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_greater", Syntax.TypeReference(PrimitiveType.I32))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectReturn, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Greater_InvalidParameterCount()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_greater", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameterCount, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Greater_InvalidParameter_0()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_greater", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameter, model.Report.Messages.First().Code);
        }



        [TestMethod]
        public void ReportSpecialOperatorMessages_GreaterEqual_NonGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_greaterequal", Syntax.TypeReference(PrimitiveType.Bool))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorMustBeGlobal, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_GreaterEqual_InvalidReturn()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_greaterequal", Syntax.TypeReference(PrimitiveType.I32))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectReturn, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_GreaterEqual_InvalidParameterCount()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_greaterequal", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameterCount, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_GreaterEqual_InvalidParameter_0()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_greaterequal", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameter, model.Report.Messages.First().Code);
        }



        [TestMethod]
        public void ReportSpecialOperatorMessages_Less_NonGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_less", Syntax.TypeReference(PrimitiveType.Bool))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorMustBeGlobal, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Less_InvalidReturn()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_less", Syntax.TypeReference(PrimitiveType.I32))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectReturn, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Less_InvalidParameterCount()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_less", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameterCount, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_Less_InvalidParameter_0()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_less", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameter, model.Report.Messages.First().Code);
        }



        [TestMethod]
        public void ReportSpecialOperatorMessages_LessEqual_NonGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_lessequal", Syntax.TypeReference(PrimitiveType.Bool))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorMustBeGlobal, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_LessEqual_InvalidReturn()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_lessequal", Syntax.TypeReference(PrimitiveType.I32))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectReturn, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_LessEqual_InvalidParameterCount()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_lessequal", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameterCount, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportSpecialOperatorMessages_LessEqual_InvalidParameter_0()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("op_lessequal", Syntax.TypeReference(PrimitiveType.Bool))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithBody()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameter, model.Report.Messages.First().Code);
        }
    }
}
