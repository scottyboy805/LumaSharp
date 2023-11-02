using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp_Compiler.Semantics.Model.Expression;

namespace LumaSharp_CompilerTests.Semantic.StaticAnalysis
{
    [TestClass]
    public sealed class StaticAnalyseBinaryUnitTests
    {
        [TestMethod]
        public void StaticAnalyse_InferredType_I32_Add_I32_I32()
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
            Assert.AreEqual(PrimitiveType.I32, binary.EvaluatedTypeSymbol.PrimitiveType);
        }

        [TestMethod]
        public void StaticAnalyse_InferredType_I64_Add_I32_I64()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test", Syntax.TypeReference(PrimitiveType.I32))
                .WithStatements(Syntax.Return(Syntax.Binary(Syntax.Literal(5), BinaryOperation.Add, Syntax.Literal(10L))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            BinaryModel binary = model.DescendantsOfType<BinaryModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(binary);
            Assert.AreEqual(PrimitiveType.I64, binary.EvaluatedTypeSymbol.PrimitiveType);
        }

        [TestMethod]
        public void StaticAnalyse_InferredType_I64_Add_I32_U32()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test", Syntax.TypeReference(PrimitiveType.I32))
                .WithStatements(Syntax.Return(Syntax.Binary(Syntax.Literal(5), BinaryOperation.Add, Syntax.Literal(10U))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            BinaryModel binary = model.DescendantsOfType<BinaryModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(binary);
            Assert.AreEqual(PrimitiveType.I64, binary.EvaluatedTypeSymbol.PrimitiveType);
        }

        [TestMethod]
        public void StaticAnalyse_InferredType_I64_Add_I32_U64()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test", Syntax.TypeReference(PrimitiveType.I32))
                .WithStatements(Syntax.Return(Syntax.Binary(Syntax.Literal(5), BinaryOperation.Add, Syntax.Literal(10UL))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            BinaryModel binary = model.DescendantsOfType<BinaryModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(binary);
            Assert.AreEqual(PrimitiveType.U64, binary.EvaluatedTypeSymbol.PrimitiveType);
        }
    }
}
