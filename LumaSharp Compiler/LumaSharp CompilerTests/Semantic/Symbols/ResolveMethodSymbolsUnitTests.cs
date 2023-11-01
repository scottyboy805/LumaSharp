using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Semantics.Model.Expression;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.Symbols
{
    [TestClass]
    public sealed class ResolveMethodSymbolsUnitTests
    {
        [TestMethod]
        public void ResolveMethodSymbols_ParameterLess()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test")
                .WithMembers(Syntax.Method("myMethod", Syntax.TypeReference(PrimitiveType.I32)),
                Syntax.Method("Test")
                .WithStatements(Syntax.Assign(Syntax.MethodInvoke("myMethod", Syntax.This()), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodInvokeModel methodModel = model.DescendantsOfType<MethodInvokeModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.IsNotNull(methodModel.EvaluatedTypeSymbol);
            Assert.IsNotNull(methodModel.AccessModelExpression.EvaluatedTypeSymbol);
            Assert.AreEqual("Test", methodModel.AccessModelExpression.EvaluatedTypeSymbol.TypeName); // `this` should be mapped to `Test`
            Assert.AreEqual(0, model.Report.MessageCount);
        }

        [TestMethod]
        public void ResolveMethodSymbols_ParameterLess_Global()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test")
                .WithMembers(Syntax.Method("myMethod", Syntax.TypeReference(PrimitiveType.I32))
                .WithAccessModifiers("global"),
                Syntax.Method("Test")
                .WithStatements(Syntax.Assign(Syntax.MethodInvoke("myMethod", Syntax.TypeReference("Test")), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodInvokeModel methodModel = model.DescendantsOfType<MethodInvokeModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.IsNotNull(methodModel.EvaluatedTypeSymbol);
            Assert.IsNotNull(methodModel.AccessModelExpression.EvaluatedTypeSymbol);
            Assert.AreEqual("Test", methodModel.AccessModelExpression.EvaluatedTypeSymbol.TypeName); // `this` should be mapped to `Test`
            Assert.AreEqual(0, model.Report.MessageCount);
        }

        [TestMethod]
        public void ResolveMethodSymbols_Parameter_1()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test")
                .WithMembers(Syntax.Method("myMethod", Syntax.TypeReference(PrimitiveType.I32))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "arg")),
                Syntax.Method("Test")
                .WithStatements(Syntax.Assign(Syntax.MethodInvoke("myMethod", Syntax.TypeReference("Test"))
                .WithArguments(Syntax.Literal(5)), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodInvokeModel methodModel = model.DescendantsOfType<MethodInvokeModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.IsNotNull(methodModel.EvaluatedTypeSymbol);
            Assert.IsNotNull(methodModel.AccessModelExpression.EvaluatedTypeSymbol);
            Assert.AreEqual("Test", methodModel.AccessModelExpression.EvaluatedTypeSymbol.TypeName); // `this` should be mapped to `Test`
            Assert.AreEqual(0, model.Report.MessageCount);
        }

        [TestMethod]
        public void ResolveMethodSymbols_Parameter_1Type()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test")
                .WithMembers(Syntax.Method("myMethod", Syntax.TypeReference(PrimitiveType.I32))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("Test"), "arg")),
                Syntax.Method("Test")
                .WithStatements(Syntax.Assign(Syntax.MethodInvoke("myMethod", Syntax.TypeReference("Test"))
                .WithArguments(Syntax.New(Syntax.TypeReference("Test"), false)), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodInvokeModel methodModel = model.DescendantsOfType<MethodInvokeModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.IsNotNull(methodModel.EvaluatedTypeSymbol);
            Assert.IsNotNull(methodModel.AccessModelExpression.EvaluatedTypeSymbol);
            Assert.AreEqual("Test", methodModel.AccessModelExpression.EvaluatedTypeSymbol.TypeName); // `this` should be mapped to `Test`
            Assert.AreEqual(0, model.Report.MessageCount);
        }

        [TestMethod]
        public void ResolveMethodSymbols_Parameter_2()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test")
                .WithMembers(Syntax.Method("myMethod", Syntax.TypeReference(PrimitiveType.I32))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "arg0"),
                Syntax.Parameter(Syntax.TypeReference(PrimitiveType.Any), "arg1")),
                Syntax.Method("Test")
                .WithStatements(Syntax.Assign(Syntax.MethodInvoke("myMethod", Syntax.TypeReference("Test"))
                .WithArguments(Syntax.Literal(5), Syntax.Literal("Hello")), Syntax.Literal(5)))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            MethodInvokeModel methodModel = model.DescendantsOfType<MethodInvokeModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(methodModel);
            Assert.IsNotNull(methodModel.EvaluatedTypeSymbol);
            Assert.IsNotNull(methodModel.AccessModelExpression.EvaluatedTypeSymbol);
            Assert.AreEqual("Test", methodModel.AccessModelExpression.EvaluatedTypeSymbol.TypeName); // `this` should be mapped to `Test`
            Assert.AreEqual(0, model.Report.MessageCount);
        }
    }
}
