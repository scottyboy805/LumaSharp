using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.Semantic.Reporting
{
    [TestClass]
    public sealed class ReportGenericArgumentUnitTests
    {
        [TestClass]
        public sealed class ReportGenericParameterUnitTests
        {
            [TestMethod]
            public void ReportGenericArgumentMessages_Generic_Empty()
            {
                SyntaxTree tree = SyntaxTree.Create(
                    Syntax.Type("Test")
                    .WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test", Syntax.GenericArgumentList(Syntax.TypeReference(PrimitiveType.I32))))));

                // Create model
                SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

                Console.WriteLine(model.Report);

                Assert.IsNotNull(model);
                Assert.AreEqual(1, model.Report.DiagnosticCount);
                Assert.AreEqual((int)Code.InvalidNoGenericArgument, model.Report.Diagnostics.First().Code);
            }

            [TestMethod]
            public void ReportGenericArgumentMessages_Generic_InvalidCount()
            {
                SyntaxTree tree = SyntaxTree.Create(
                    Syntax.Type("Test").WithGenericParameters(Syntax.GenericParameter("T"), Syntax.GenericParameter("J"))
                    .WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test", Syntax.GenericArgumentList(Syntax.TypeReference(PrimitiveType.I32))))));

                // Create model
                SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

                Console.WriteLine(model.Report);

                Assert.IsNotNull(model);
                Assert.AreEqual(1, model.Report.DiagnosticCount);
                Assert.AreEqual((int)Code.InvalidCountGenericArgument, model.Report.Diagnostics.First().Code);
            }

            [TestMethod]
            public void ReportGenericArgumentMessages_Generic_InvalidConstraint()
            {
                SyntaxTree tree = SyntaxTree.Create(
                    Syntax.Type("Test").WithGenericParameters(Syntax.GenericParameter("T", Syntax.TypeReference("Test")))
                    .WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test", Syntax.GenericArgumentList(Syntax.TypeReference(PrimitiveType.I32))))));

                // Create model
                SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

                Console.WriteLine(model.Report);

                Assert.IsNotNull(model);
                Assert.AreEqual(1, model.Report.DiagnosticCount);
                Assert.AreEqual((int)Code.InvalidConstraintGenericArgument, model.Report.Diagnostics.First().Code);
            }
        }
    }
}
