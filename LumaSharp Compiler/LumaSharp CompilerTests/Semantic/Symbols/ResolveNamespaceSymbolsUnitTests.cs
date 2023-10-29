using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.Symbols
{
    [TestClass]
    public sealed class ResolveNamespaceSymbolsUnitTests
    {
        [TestMethod]
        public void ResolveNamespaceSymbols_Import()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Import("MyNamespaceh"),
                Syntax.Namespace("MyNamespace"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Report.MessageCount);
        }

        [TestMethod]
        public void ResolveNamespaceSymbols_NamedType()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Namespace("MyNamespace")
                .WithRootMembers(Syntax.Type("Test")
                .WithMembers(Syntax.Method("M")
                .WithStatements(Syntax.Return(Syntax.TypeReference("Test")
                .WithNamespaceQualifier("MyNamespace"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Report.MessageCount);
        }

        [TestMethod]
        public void ResolveNamespaceSymbols_NamedTypeNested()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Namespace("MyNamespace:MySubNamespace:MyFinalNamespace")
                .WithRootMembers(Syntax.Type("Test")
                .WithMembers(Syntax.Method("M")
                .WithStatements(Syntax.Return(Syntax.TypeReference("Test")
                .WithNamespaceQualifier("MyNamespace", "MySubNamespace", "MyFinalNamespace"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Report.MessageCount);
        }
    }
}
