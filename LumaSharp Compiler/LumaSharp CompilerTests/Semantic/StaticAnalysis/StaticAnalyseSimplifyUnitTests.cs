using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.StaticAnalysis
{
    [TestClass]
    public sealed class StaticAnalyseSimplifyUnitTests
    {
        [TestMethod]
        public void StaticAnalyseSimplify_SimpleAdd()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test", Syntax.TypeReference(PrimitiveType.I32))
                .WithBody(Syntax.Return(Syntax.Binary(Syntax.Literal(5), BinaryOperation.Add, Syntax.Literal(10))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            BinaryModel binary = model.DescendantsOfType<BinaryModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(binary);

            model.StaticallyEvaluate();
            ConstantModel simplified = model.DescendantsOfType<ConstantModel>(true).FirstOrDefault();

            Assert.IsNotNull(simplified);
            Assert.AreEqual(PrimitiveType.I32, simplified.EvaluatedTypeSymbol.PrimitiveType);
            Assert.AreEqual(15, simplified.GetStaticallyEvaluatedValue());
        }

        [TestMethod]
        public void StaticAnalyseSimplify_SimpleAddSubtract()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test", Syntax.TypeReference(PrimitiveType.I32))
                .WithBody(Syntax.Return(Syntax.Binary(Syntax.Literal(5), BinaryOperation.Add, Syntax.Binary(Syntax.Literal(10), BinaryOperation.Subtract, Syntax.Literal(2)))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            BinaryModel binary = model.DescendantsOfType<BinaryModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(binary);

            model.StaticallyEvaluate();
            ConstantModel simplified = model.DescendantsOfType<ConstantModel>(true).FirstOrDefault();

            Assert.IsNotNull(simplified);
            Assert.AreEqual(PrimitiveType.I32, simplified.EvaluatedTypeSymbol.PrimitiveType);
            Assert.AreEqual(13, simplified.GetStaticallyEvaluatedValue());
        }
    }
}
