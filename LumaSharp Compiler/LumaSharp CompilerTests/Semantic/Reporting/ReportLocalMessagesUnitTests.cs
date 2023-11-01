using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.Reporting
{
    [TestClass]
    public sealed class ReportLocalMessagesUnitTests
    {
        [TestMethod]
        public void ReportLocalMessages_MissingIdentifier()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Return(Syntax.VariableReference("var")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.IdentifierNotFound, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportLocalMessages_UsedBeforeDeclared()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Assign(Syntax.VariableReference("var"), Syntax.Literal(5)),
                Syntax.Variable(Syntax.TypeReference(PrimitiveType.I32), "var"))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.IdentifierUsedBeforeDeclared, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportLocalMessages_MultipleLocals_2()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.I32), "var"),
                Syntax.Variable(Syntax.TypeReference(PrimitiveType.Bool), "var"))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.MultipleLocalIdentifiers, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportLocalMessages_MultipleLocals_3()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithStatements(Syntax.Variable(Syntax.TypeReference(PrimitiveType.I32), "var"),
                Syntax.Variable(Syntax.TypeReference(PrimitiveType.Bool), "var"),
                Syntax.Variable(Syntax.TypeReference(PrimitiveType.Any), "var"))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Report.MessageCount);
            Assert.AreEqual((int)Code.MultipleLocalIdentifiers, model.Report.Messages.First().Code);
        }
    }
}
