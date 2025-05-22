using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Compiler.Reporting;

namespace CompilerTests.Semantic.Reporting
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
                .WithBody(Syntax.Assign(Syntax.MemberReference(Syntax.This(), "myField"),
                    Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.Literal(5))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.FieldAccessorNotFound, model.Report.Diagnostics.First().Code);
        }

        [TestMethod]
        public void ReportFieldMessages_FieldRequiresInstance()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Field("myField", Syntax.TypeReference("Test")),
                Syntax.Method("Test", Syntax.TypeReference("Test"))
                .WithBody(Syntax.Return(Syntax.MemberReference(Syntax.TypeReference("Test"), "myField")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.FieldRequiresInstance, model.Report.Diagnostics.First().Code);
        }

        [TestMethod]
        public void ReportFieldMessages_FieldRequiresType_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword)),
                Syntax.Method("Test", Syntax.TypeReference("Test"))
                .WithBody(Syntax.Return(Syntax.MemberReference(Syntax.This(), "myField")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.FieldRequiresType, model.Report.Diagnostics.First().Code);
        }

        [TestMethod]
        public void ReportFieldMessages_FieldRequiresType_2()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword)),
                Syntax.Method("Test", Syntax.TypeReference("Test"))
                .WithBody(Syntax.LocalVariable(Syntax.TypeReference("Test"), "myVar"),
                    Syntax.Return(Syntax.MemberReference(Syntax.VariableReference("myVar"), "myField")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.FieldRequiresType, model.Report.Diagnostics.First().Code);
        }

        [TestMethod]
        public void ReportFieldMessages_InvalidFieldAssign()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Field("myField", Syntax.TypeReference("Test"), Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.InvalidConversion, model.Report.Diagnostics.First().Code);
        }
    }
}
