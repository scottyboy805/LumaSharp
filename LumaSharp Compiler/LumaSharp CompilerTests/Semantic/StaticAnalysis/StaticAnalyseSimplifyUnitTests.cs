using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Semantics.Model.Expression;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.StaticAnalysis
{
    [TestClass]
    public sealed class StaticAnalyseSimplifyUnitTests
    {
        [TestMethod]
        public void StaticAnalyseSimplify_Return()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test", Syntax.TypeReference(PrimitiveType.I32))
                .WithStatements(Syntax.Return(Syntax.Binary(Syntax.Literal(5), BinaryOperation.Add, Syntax.Literal(10))))));

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
    }
}
