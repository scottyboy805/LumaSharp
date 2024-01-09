using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model;
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
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.InvalidPrimitiveGenericConstraint, model.Report.Messages.First().Code);
        }
    }
}
