using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.Symbols
{
    [TestClass]
    public sealed class ResolveGenericParameterSymbolsUnitTests
    {
        [TestMethod]
        public void ResolveGenericParameterSymbols_MethodReturn()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("test")
                .WithMembers(Syntax.Method("test", Syntax.TypeReference("T"))
                .WithGenericParameters(Syntax.GenericParameter("T"))
                .WithBody(Syntax.Return(Syntax.New(Syntax.TypeReference("T"))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.TypeModels.Count);
            Assert.AreEqual(0, model.Report.DiagnosticCount);
        }

        [TestMethod]
        public void ResolveGenericParameterSymbols_TypeReturn()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("test")
                .WithGenericParameters(Syntax.GenericParameter("T"))
                .WithMembers(Syntax.Method("test")                
                .WithBody(Syntax.Variable(Syntax.TypeReference("T"), "var"))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.TypeModels.Count);
            Assert.AreEqual(0, model.Report.DiagnosticCount);
        }

        [TestMethod]
        public void ResolveGenericParameterSymbols_SubTypeReturn()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("test")
                .WithGenericParameters(Syntax.GenericParameter("T"))
                .WithMembers(Syntax.Type("Sub")
                .WithGenericParameters(Syntax.GenericParameter("M"))
                .WithMembers(Syntax.Method("test")
                .WithBody(Syntax.Variable(Syntax.TypeReference("T"), "var1"),
                Syntax.Variable(Syntax.TypeReference("M"), "var2")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.TypeModels.Count);
            Assert.AreEqual(0, model.Report.DiagnosticCount);
        }
    }
}
