using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp_CompilerTests.Semantic.Reporting
{
    [TestClass]
    public sealed class ReportFieldMessagesUnitTests
    {
        [TestMethod]
        public void ReportFieldMessages_MissingField()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Assign(Syntax.FieldReference(Syntax.This(), "myField"), AssignOperation.Assign, Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.FieldAccessorNotFound, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportFieldMessages_FieldRequiresInstance()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Field("myField", Syntax.TypeReference("Test")),
                Syntax.Method("Test", Syntax.TypeReference("Test"))
                .WithBody(Syntax.Return(Syntax.FieldReference(Syntax.TypeReference("Test"), "myField")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.FieldRequiresInstance, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportFieldMessages_FieldRequiresType_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword)),
                Syntax.Method("Test", Syntax.TypeReference("Test"))
                .WithBody(Syntax.Return(Syntax.FieldReference(Syntax.This(), "myField")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.FieldRequiresType, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportFieldMessages_FieldRequiresType_2()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword)),
                Syntax.Method("Test", Syntax.TypeReference("Test"))
                .WithBody(Syntax.Variable(Syntax.TypeReference("Test"), "myVar"),
                    Syntax.Return(Syntax.FieldReference(Syntax.VariableReference("myVar"), "myField")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.FieldRequiresType, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportFieldMessages_InvalidFieldAssign()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Field("myField", Syntax.TypeReference("Test"), Syntax.VariableAssignment(AssignOperation.Assign, Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.InvalidConversion, model.Report.Messages.First().Code);
        }
    }
}
