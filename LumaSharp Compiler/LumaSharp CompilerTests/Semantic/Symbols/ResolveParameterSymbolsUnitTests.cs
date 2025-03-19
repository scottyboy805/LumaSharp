using LumaSharp.Compiler.Semantics.Model;
using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.Symbols
{
    [TestClass]
    public class ResolveParameterSymbolsUnitTests
    {
        [TestMethod]
        public void ResolveParameterSymbols_Local()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("MyType")
                    .WithMembers(Syntax.Method("MyMethod", Syntax.TypeReference(PrimitiveType.I32))
                        .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "myParam"))
                        .WithBody(
                            Syntax.Variable(Syntax.TypeReference(PrimitiveType.I32), "myVar"),
                            Syntax.Assign(
                                Syntax.VariableReference("myVar"), AssignOperation.Assign,
                                Syntax.Literal(5)))));

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
        public void ResolveParameterSymbols_Return()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("test")
                .WithMembers(Syntax.Method("test", Syntax.TypeReference("test"))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference("test"), "myVal"))
                .WithBody(Syntax.Return(Syntax.VariableReference("myVal")))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.TypeModels.Count);
            Assert.AreEqual(0, model.Report.MessageCount);
        }
    }
}
