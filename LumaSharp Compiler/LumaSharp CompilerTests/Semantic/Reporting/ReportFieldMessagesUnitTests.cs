using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Semantics.Model.Expression;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp_Compiler.Reporting;

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
                .WithStatements(Syntax.Assign(Syntax.FieldReference("myField", Syntax.This()), Syntax.Literal(5)))));

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
                .WithStatements(Syntax.Return(Syntax.FieldReference("myField", Syntax.TypeReference("Test"))))));

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
                .WithAccessModifiers("global"),
                Syntax.Method("Test", Syntax.TypeReference("Test"))
                .WithStatements(Syntax.Return(Syntax.FieldReference("myField", Syntax.This())))));

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
                .WithAccessModifiers("global"),
                Syntax.Method("Test", Syntax.TypeReference("Test"))
                .WithStatements(Syntax.Variable(Syntax.TypeReference("Test"), "myVar"),
                    Syntax.Return(Syntax.FieldReference("myField", Syntax.VariableReference("myVar"))))));

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
                Syntax.Field("myField", Syntax.TypeReference("Test"), Syntax.Literal(5))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.InvalidConversion, model.Report.Messages.First().Code);
        }
    }
}
