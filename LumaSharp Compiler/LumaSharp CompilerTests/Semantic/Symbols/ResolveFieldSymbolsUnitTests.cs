using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp_Compiler.Semantics.Model.Statement;
using LumaSharp_Compiler.Semantics.Model.Expression;

namespace LumaSharp_CompilerTests.Semantic.Symbols
{
    [TestClass]
    public sealed class ResolveFieldSymbolsUnitTests
    {
        [TestMethod]
        public void ResolveFieldSymbols_LocalThis()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test")
                .WithMembers(Syntax.Field("myField", Syntax.TypeReference(PrimitiveType.I32)),
                Syntax.Method("Test")
                .WithStatements(Syntax.Assign(Syntax.FieldReference("myField", Syntax.This()), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            FieldAccessorReferenceModel fieldModel = model.DescendantsOfType<FieldAccessorReferenceModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(fieldModel);
            Assert.IsNotNull(fieldModel.EvaluatedTypeSymbol);
            Assert.IsNotNull(fieldModel.AccessModelExpression.EvaluatedTypeSymbol);
            Assert.AreEqual("Test", fieldModel.AccessModelExpression.EvaluatedTypeSymbol.TypeName); // `this` should be mapped to `Test`
            Assert.AreEqual(0, model.Report.MessageCount);
        }
    }
}
