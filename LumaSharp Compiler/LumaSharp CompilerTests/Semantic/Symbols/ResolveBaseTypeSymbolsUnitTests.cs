using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp_Compiler.Semantics.Reference;

namespace LumaSharp_CompilerTests.Semantic.Symbols
{
    [TestClass]
    public sealed class ResolveBaseTypeSymbolsUnitTests
    {
        [TestMethod]
        public void ResolveBaseTypeSymbols_DefaultAny()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            TypeModel typeModel = model.DescendantsOfType<TypeModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(typeModel);
            Assert.IsNotNull(typeModel.BaseTypeSymbols);
            Assert.AreEqual(1, typeModel.BaseTypeSymbols.Length);
            Assert.IsTrue(TypeChecker.IsSpecialTypeAny(typeModel.BaseTypeSymbols[0]));
            Assert.AreEqual(0, model.Report.MessageCount);
        }

        [TestMethod]
        public void ResolveBaseTypeSymbols_DefaultAnyContract_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Contract("C"),
                Syntax.Type("Test").WithBaseTypes(Syntax.TypeReference("C")));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            TypeModel typeModel = model.DescendantsOfType<TypeModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(typeModel);
            Assert.IsNotNull(typeModel.BaseTypeSymbols);
            Assert.AreEqual(2, typeModel.BaseTypeSymbols.Length);
            Assert.IsTrue(TypeChecker.IsSpecialTypeAny(typeModel.BaseTypeSymbols[0]));
            Assert.AreEqual(0, model.Report.MessageCount);
        }

        [TestMethod]
        public void ResolveBaseTypeSymbols_DefaultAnyContract_2()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Contract("C"), Syntax.Contract("C1"),
                Syntax.Type("Test").WithBaseTypes(Syntax.TypeReference("C"), Syntax.TypeReference("C1")));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            TypeModel typeModel = model.DescendantsOfType<TypeModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(typeModel);
            Assert.IsNotNull(typeModel.BaseTypeSymbols);
            Assert.AreEqual(3, typeModel.BaseTypeSymbols.Length);
            Assert.IsTrue(TypeChecker.IsSpecialTypeAny(typeModel.BaseTypeSymbols[0]));
            Assert.AreEqual(0, model.Report.MessageCount);
        }

        [TestMethod]
        public void ResolveBaseTypeSymbols_DefaultEnum()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Enum("Test"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            TypeModel typeModel = model.DescendantsOfType<TypeModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(typeModel);
            Assert.IsNotNull(typeModel.BaseTypeSymbols);
            Assert.AreEqual(1, typeModel.BaseTypeSymbols.Length);
            Assert.IsTrue(TypeChecker.IsSpecialTypeEnum(typeModel.BaseTypeSymbols[0]));
            Assert.AreEqual(0, model.Report.MessageCount);
        }
    }
}
