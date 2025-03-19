using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                .WithBody(Syntax.Assign(Syntax.FieldReference(Syntax.This(), "myField"), AssignOperation.Assign, Syntax.Literal(5)))));

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

        [TestMethod]
        public void ResolveFieldSymbols_LocalVariable()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test")
                .WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test")),
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference("Test"), "myVar"),
                    Syntax.Assign(Syntax.FieldReference(Syntax.VariableReference("myVar"), "myField"), AssignOperation.Assign, Syntax.VariableReference("myVar")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            FieldAccessorReferenceModel fieldModel = model.DescendantsOfType<FieldAccessorReferenceModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(fieldModel);
            Assert.IsNotNull(fieldModel.EvaluatedTypeSymbol);
            Assert.IsNotNull(fieldModel.AccessModelExpression.EvaluatedTypeSymbol);
            Assert.AreEqual("Test", fieldModel.AccessModelExpression.EvaluatedTypeSymbol.TypeName);
            Assert.AreEqual(0, model.Report.MessageCount);
        }

        [TestMethod]
        public void ResolveFieldSymbols_LocalType()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test")
                .WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword)),
                Syntax.Method("Test")
                .WithBody(Syntax.Variable(Syntax.TypeReference("Test"), "myVar"),
                    Syntax.Assign(Syntax.FieldReference(Syntax.TypeReference("Test"), "myField"), AssignOperation.Assign, Syntax.VariableReference("myVar")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            FieldAccessorReferenceModel fieldModel = model.DescendantsOfType<FieldAccessorReferenceModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(fieldModel);
            Assert.IsNotNull(fieldModel.EvaluatedTypeSymbol);
            Assert.IsNotNull(fieldModel.AccessModelExpression.EvaluatedTypeSymbol);
            Assert.AreEqual("Test", fieldModel.AccessModelExpression.EvaluatedTypeSymbol.TypeName);
            Assert.AreEqual(0, model.Report.MessageCount);
        }
    }
}
