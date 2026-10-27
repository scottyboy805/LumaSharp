//using LumaSharp.Compiler.AST;
//using LumaSharp.Compiler.SemanticsOld.Model;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using LumaSharp.Compiler.SemanticsOld.Reference;

//namespace CompilerTests.Semantic.Symbols
//{
//    [TestClass]
//    public sealed class ResolveBaseTypeSymbolsUnitTests
//    {
//        [TestMethod]
//        public void ResolveBaseTypeSymbols_DefaultAny()
//        {
//            SyntaxTree tree = SyntaxTree.Create(
//                Syntax.Class("Test"));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            TypeModel typeModel = model.DescendantsOfType<TypeModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(typeModel);
//            Assert.IsNotNull(typeModel.BaseTypeSymbols);
//            Assert.AreEqual(1, typeModel.BaseTypeSymbols.Length);
//            Assert.IsTrue(TypeChecker.IsSpecialTypeAny(typeModel.BaseTypeSymbols[0]));
//            Assert.AreEqual(0, model.Report.DiagnosticCount);
//        }

//        [TestMethod]
//        public void ResolveBaseTypeSymbols_DefaultAnyContract_1()
//        {
//            SyntaxTree tree = SyntaxTree.Create(
//                Syntax.Interface("C"),
//                Syntax.Class("Test").WithBaseTypes(Syntax.TypeReference("C")));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            TypeModel typeModel = model.DescendantsOfType<TypeModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(typeModel);
//            Assert.IsNotNull(typeModel.BaseTypeSymbols);
//            Assert.AreEqual(2, typeModel.BaseTypeSymbols.Length);
//            Assert.IsTrue(TypeChecker.IsSpecialTypeAny(typeModel.BaseTypeSymbols[0]));
//            Assert.AreEqual(0, model.Report.DiagnosticCount);
//        }

//        [TestMethod]
//        public void ResolveBaseTypeSymbols_DefaultAnyContract_2()
//        {
//            SyntaxTree tree = SyntaxTree.Create(
//                Syntax.Interface("C"), Syntax.Interface("C1"),
//                Syntax.Class("Test").WithBaseTypes(Syntax.TypeReference("C"), Syntax.TypeReference("C1")));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            TypeModel typeModel = model.DescendantsOfType<TypeModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(typeModel);
//            Assert.IsNotNull(typeModel.BaseTypeSymbols);
//            Assert.AreEqual(3, typeModel.BaseTypeSymbols.Length);
//            Assert.IsTrue(TypeChecker.IsSpecialTypeAny(typeModel.BaseTypeSymbols[0]));
//            Assert.AreEqual(0, model.Report.DiagnosticCount);
//        }

//        [TestMethod]
//        public void ResolveBaseTypeSymbols_DefaultEnum()
//        {
//            SyntaxTree tree = SyntaxTree.Create(
//                Syntax.Enum("Test"));

//            // Create model
//            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
//            TypeModel typeModel = model.DescendantsOfType<TypeModel>(true).FirstOrDefault();

//            Assert.IsNotNull(model);
//            Assert.IsNotNull(typeModel);
//            Assert.IsNotNull(typeModel.BaseTypeSymbols);
//            Assert.AreEqual(1, typeModel.BaseTypeSymbols.Length);
//            Assert.IsTrue(TypeChecker.IsSpecialTypeEnum(typeModel.BaseTypeSymbols[0]));
//            Assert.AreEqual(0, model.Report.DiagnosticCount);
//        }
//    }
//}
