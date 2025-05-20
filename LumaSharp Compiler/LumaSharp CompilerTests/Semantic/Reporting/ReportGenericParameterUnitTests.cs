using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.Reporting
{
    [TestClass]
    public sealed class ReportGenericParameterUnitTests
    {
        [TestMethod]
        public void ReportGenericParameterMessages_Constraint_Primitive()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithGenericParameters(Syntax.GenericParameter("T", Syntax.TypeReference(PrimitiveType.I32))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.DiagnosticCount);
            Assert.AreEqual((int)Code.InvalidPrimitiveGenericConstraint, model.Report.Diagnostics.First().Code);
        }
    }
}
