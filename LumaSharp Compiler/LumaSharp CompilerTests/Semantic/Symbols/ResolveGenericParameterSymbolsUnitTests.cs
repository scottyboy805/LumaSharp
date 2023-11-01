using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Semantics.Model;
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
                .WithStatements(Syntax.Return(Syntax.New(Syntax.TypeReference("T"), false)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.TypeModels.Count);
            Assert.AreEqual(0, model.Report.MessageCount);
        }

        [TestMethod]
        public void ResolveGenericParameterSymbols_TypeReturn()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("test")
                .WithGenericParameters(Syntax.GenericParameter("T"))
                .WithMembers(Syntax.Method("test")                
                .WithStatements(Syntax.Return(Syntax.VariableReference("T")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.TypeModels.Count);
            Assert.AreEqual(0, model.Report.MessageCount);
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
                .WithStatements(Syntax.Return(Syntax.Binary(Syntax.VariableReference("T"), BinaryOperation.Add, Syntax.VariableReference("M")))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.TypeModels.Count);
            Assert.AreEqual(0, model.Report.MessageCount);
        }
    }
}
