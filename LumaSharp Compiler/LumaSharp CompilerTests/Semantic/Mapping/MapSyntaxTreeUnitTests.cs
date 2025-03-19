using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Compiler.Semantics.Model;

namespace LumaSharp_CompilerTests.Semantic.Mapping
{
    [TestClass]
    public sealed class MapSyntaxTreeUnitTests
    {
        [TestMethod]
        public void MapSyntaxTree_Namespace()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Namespace("MyNamespace"),
                Syntax.Type("MyType"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.TypeModels.Count);

            // Get first
            TypeModel type = model.TypeModels[0];

            Assert.AreEqual("MyType", type.TypeName);
            Assert.AreEqual("MyNamespace", type.NamespaceName[0]);
            Assert.IsTrue(type.IsType);
            Assert.IsFalse(type.IsContract);
            Assert.IsFalse(type.IsEnum);
            Assert.IsFalse(type.IsPrimitive);
        }

        [TestMethod]
        public void MapSyntaxTree_Type()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("MyType"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.TypeModels.Count);

            // Get first
            TypeModel type = model.TypeModels[0];

            Assert.AreEqual("MyType", type.TypeName);
            Assert.IsTrue(type.IsType);
            Assert.IsFalse(type.IsContract);
            Assert.IsFalse(type.IsEnum);
            Assert.IsFalse(type.IsPrimitive);
        }

        [TestMethod]
        public void MapSyntaxTree_Contract()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Contract("MyContract"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.TypeModels.Count);

            // Get first
            TypeModel type = model.TypeModels[0];

            Assert.AreEqual("MyContract", type.TypeName);
            Assert.IsFalse(type.IsType);
            Assert.IsTrue(type.IsContract);
            Assert.IsFalse(type.IsEnum);
            Assert.IsFalse(type.IsPrimitive);
        }

        [TestMethod]
        public void MapSyntaxTree_Enum()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Enum("MyEnum"));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.TypeModels.Count);

            // Get first
            TypeModel type = model.TypeModels[0];

            Assert.AreEqual("MyEnum", type.TypeName);
            Assert.IsFalse(type.IsType);
            Assert.IsFalse(type.IsContract);
            Assert.IsTrue(type.IsEnum);
            Assert.IsFalse(type.IsPrimitive);
        }
    }
}
