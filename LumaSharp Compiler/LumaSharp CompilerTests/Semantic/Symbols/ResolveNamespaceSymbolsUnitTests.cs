//using LumaSharp.Compiler.AST;
//using LumaSharp.Compiler.Semantics.Model;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace LumaSharp_CompilerTests.Semantic.Symbols
//{
//    [TestClass]
//    public sealed class ResolveNamespaceSymbolsUnitTests
//    {
//        [TestMethod]
//        public void ResolveNamespaceSymbols_Import()
//        {
//            SyntaxTree tree = SyntaxTree.Create(
//                Syntax.Import("MyNamespace"),
//                Syntax.Namespace("MyNamespace"));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

//            Assert.IsNotNull(model);
//            Assert.AreEqual(0, model.Report.MessageCount);
//        }

//        [TestMethod]
//        public void ResolveNamespaceSymbols_NamedType()
//        {
//            SyntaxTree tree = SyntaxTree.Create(
//                Syntax.Namespace("MyNamespace")
//                .WithRootMembers(Syntax.Type("Test")
//                .WithMembers(Syntax.Method("M", Syntax.TypeReference("Test"))
//                .WithBody(Syntax.Return(Syntax.TypeReference(new string[] { "MyNamespace" }, "Test"))))));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            ReturnModel returnModel = model.DescendantsOfType<ReturnModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(returnModel);
//            Assert.IsNotNull(returnModel.ReturnModelExpressions[0].EvaluatedTypeSymbol);
//            Assert.AreEqual("Test", returnModel.ReturnModelExpressions[0].EvaluatedTypeSymbol.TypeName);
//            Assert.AreEqual(0, model.Report.MessageCount);
//        }

//        [TestMethod]
//        public void ResolveNamespaceSymbols_NamedTypeNested()
//        {
//            SyntaxTree tree = SyntaxTree.Create(
//                Syntax.Namespace("MyNamespace", "MySubNamespace", "MyFinalNamespace")
//                .WithRootMembers(Syntax.Type("Test")
//                .WithMembers(Syntax.Method("M", Syntax.TypeReference("Test"))
//                .WithBody(Syntax.Return(Syntax.TypeReference(new string[] { "MyNamespace", "MySubNamespace", "MyFinalNamespace" }, "Test"))))));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            ReturnModel returnModel = model.DescendantsOfType<ReturnModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(returnModel);
//            Assert.IsNotNull(returnModel.ReturnModelExpressions[0].EvaluatedTypeSymbol);
//            Assert.AreEqual("Test", returnModel.ReturnModelExpressions[0].EvaluatedTypeSymbol.TypeName);
//            Assert.AreEqual(0, model.Report.MessageCount);
//        }
//    }
//}
