using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.Reporting
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
                    .WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test").WithGenericArguments(Syntax.TypeReference(PrimitiveType.I32)))));

                // Create model
                SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

                Assert.IsNotNull(model);
                Assert.AreEqual(1, model.Report.MessageCount);
                Assert.AreEqual((int)Code.InvalidNoGenericArgument, model.Report.Messages.First().Code);
            }

            [TestMethod]
            public void ReportGenericArgumentMessages_Generic_InvalidCount()
            {
                SyntaxTree tree = SyntaxTree.Create(
                    Syntax.Type("Test").WithGenericParameters(Syntax.GenericParameter("T"), Syntax.GenericParameter("J"))
                    .WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test").WithGenericArguments(Syntax.TypeReference(PrimitiveType.I32)))));

                // Create model
                SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

                Assert.IsNotNull(model);
                Assert.AreEqual(1, model.Report.MessageCount);
                Assert.AreEqual((int)Code.InvalidCountGenericArgument, model.Report.Messages.First().Code);
            }

            [TestMethod]
            public void ReportGenericArgumentMessages_Generic_InvalidConstraint()
            {
                SyntaxTree tree = SyntaxTree.Create(
                    Syntax.Type("Test").WithGenericParameters(Syntax.GenericParameter("T", Syntax.TypeReference("Test")))
                    .WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test").WithGenericArguments(Syntax.TypeReference(PrimitiveType.I32)))));

                // Create model
                SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

                Assert.IsNotNull(model);
                Assert.AreEqual(1, model.Report.MessageCount);
                Assert.AreEqual((int)Code.InvalidConstraintGenericArgument, model.Report.Messages.First().Code);
            }
        }
    }
}
