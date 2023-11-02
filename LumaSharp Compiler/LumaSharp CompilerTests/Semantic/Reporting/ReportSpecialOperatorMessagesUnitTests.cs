using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model;
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
                .WithStatements()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

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
                .WithAccessModifiers("global")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithStatements()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

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
                .WithAccessModifiers("global")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"))
                .WithStatements()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

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
                .WithAccessModifiers("global")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "a"), Syntax.Parameter(Syntax.TypeReference("Test"), "b"))
                .WithStatements()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

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
                .WithAccessModifiers("global")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "a"), Syntax.Parameter(Syntax.TypeReference(PrimitiveType.Bool), "b"))
                .WithStatements()));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.OperatorIncorrectParameter, model.Report.Messages.First().Code);
        }
    }
}
