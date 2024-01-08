using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.Reporting
{
    public sealed class ReportGenericParameterUnitTests
    {
        [TestMethod]
        public void ReportGenericParameterMessages_Type_InheritanceCycle()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithBaseTypes(Syntax.TypeReference("Test")));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.InvalidSelfBaseType, model.Report.Messages.First().Code);
        }
    }
}
