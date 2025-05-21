using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.Semantic.Reporting
{
    [TestClass]
    public sealed class ReportKeywordMessagesUnitTests
    {
        [TestMethod]
        public void ReportKeywordMessages_InvalidThis_GlobalMethod()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithBody(Syntax.Return(Syntax.This()))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.KeywordNotValid, model.Report.Diagnostics.First().Code);
        }

        [TestMethod]
        public void ReportKeywordMessages_InvalidThis_Field()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Field("myField", Syntax.TypeReference("Test"), Syntax.VariableAssignment(Syntax.This()))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.KeywordNotValid, model.Report.Diagnostics.First().Code);
        }
    }
}
