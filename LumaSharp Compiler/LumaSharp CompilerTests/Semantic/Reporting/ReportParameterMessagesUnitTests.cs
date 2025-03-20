using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.Reporting
{
    [TestClass]
    public sealed class ReportParameterMessagesUnitTests
    {
        [TestMethod]
        public void ReportParameterMessages_MissingIdentifier()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithBody(Syntax.Return(Syntax.VariableReference("var")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.IdentifierNotFound, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportParameterMessages_MultipleParameters_2()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "par"),
                Syntax.Parameter(Syntax.TypeReference(PrimitiveType.Bool), "par"))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.MultipleParameterIdentifiers, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportParameterMessages_MultipleParameters_3()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "par"),
                Syntax.Parameter(Syntax.TypeReference(PrimitiveType.Bool), "par"),
                Syntax.Parameter(Syntax.TypeReference(PrimitiveType.Any), "par"))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Report.MessageCount);
            Assert.AreEqual((int)Code.MultipleParameterIdentifiers, model.Report.Messages.First().Code);
        }
    }
}
