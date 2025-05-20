using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.Reporting
{
    [TestClass]
    public sealed class ReportBaseTypeUnitTests
    {
        [TestMethod]
        public void ReportBaseTypeMessages_Type_InheritanceCycle()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithBaseTypes(Syntax.TypeReference("Test")));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.InvalidSelfBaseType, model.Report.Diagnostics.First().Code);
        }

        [TestMethod]
        public void ReportBaseTypeMessages_Type_Enum()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Enum("MyEnum"),
                Syntax.Type("Test").WithBaseTypes(Syntax.TypeReference("MyEnum")));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.InvalidEnumBaseType, model.Report.Diagnostics.First().Code);
        }

        [TestMethod]
        public void ReportBaseTypeMessages_Type_MultipleBase()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("MyType"), Syntax.Type("MyOtherType"),
                Syntax.Type("Test").WithBaseTypes(Syntax.TypeReference("MyType"), Syntax.TypeReference("MyOtherType")));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.MultipleBaseTypes, model.Report.Diagnostics.First().Code);
        }

        [TestMethod]
        public void ReportBaseTypeMessages_Type_BaseTypeFirst()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Contract("MyContract"), Syntax.Type("MyOtherType"),
                Syntax.Type("Test").WithBaseTypes(Syntax.TypeReference("MyContract"), Syntax.TypeReference("MyOtherType")));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.FirstBaseType, model.Report.Diagnostics.First().Code);
        }

        [TestMethod]
        public void ReportBaseTypeMessages_Contract_InheritanceCycle()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Contract("Test").WithBaseTypes(Syntax.TypeReference("Test")));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.InvalidSelfBaseContract, model.Report.Diagnostics.First().Code);
        }

        [TestMethod]
        public void ReportBaseTypeMessages_Contract_Enum()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Enum("MyEnum"),
                Syntax.Contract("Test").WithBaseTypes(Syntax.TypeReference("MyEnum")));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.InvalidEnumBaseContract, model.Report.Diagnostics.First().Code);
        }

        [TestMethod]
        public void ReportBaseTypeMessages_Contract_Type()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("MyType"),
                Syntax.Contract("Test").WithBaseTypes(Syntax.TypeReference("MyType")));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.InvalidTypeBaseContract, model.Report.Diagnostics.First().Code);
        }
    }
}
