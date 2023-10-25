using LumaSharp_Compiler.AST;
using LumaSharp_Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp_Compiler.AST.Factory;

namespace LumaSharp_CompilerTests.AST.ParseGenerateSource.FromSyntax
{
    [TestClass]
    public class SyntaxGenerateExpressionUnitTests
    {
        [TestMethod]
        public void GenerateExpression_Base()
        {
            SyntaxNode syntax = Syntax.Base();

            // Get expression text
            Assert.AreEqual("base", syntax.GetSourceText());
        }

        [TestMethod]
        public void GenerateExpression_This()
        {
            SyntaxNode syntax = Syntax.This();

            // Get expression text
            Assert.AreEqual("this", syntax.GetSourceText());
        }

        [TestMethod]
        public void GenerateExpression_Type()
        {
            SyntaxNode syntax0 = Syntax.TypeOp(
                Syntax.TypeReference(PrimitiveType.I32));

            // Get expression text
            Assert.AreEqual("type(i32)", syntax0.GetSourceText());

            SyntaxNode syntax1 = Syntax.TypeOp(
                Syntax.TypeReference("MyType"));

            // Get expression text
            Assert.AreEqual("type(MyType)", syntax1.GetSourceText());
        }

        [TestMethod]
        public void GenerateExpression_Size()
        {
            SyntaxNode syntax0 = Syntax.SizeOp(
                Syntax.TypeReference(PrimitiveType.I32));

            // Get expression text
            Assert.AreEqual("size(i32)", syntax0.GetSourceText());

            SyntaxNode syntax1 = Syntax.SizeOp(
                Syntax.TypeReference("MyType"));

            // Get expression text
            Assert.AreEqual("size(MyType)", syntax1.GetSourceText());
        }

        [TestMethod]
        public void GenerateExpression_Literal()
        {
            SyntaxNode syntax0 = Syntax.Literal(567);

            // Get expression text
            Assert.AreEqual("567", syntax0.GetSourceText());

            SyntaxNode syntax1 = Syntax.Literal(123.456);

            // Get expression text
            Assert.AreEqual("123.456", syntax1.GetSourceText());

            SyntaxNode syntax2 = Syntax.Literal("Hello");

            // Get expression text
            Assert.AreEqual("Hello", syntax2.GetSourceText());

            SyntaxNode syntax3 = Syntax.Literal(false);

            Assert.AreEqual("false", syntax3.GetSourceText());
        }

        [TestMethod]
        public void GenerateExpression_Field()
        {
            SyntaxNode syntax0 = Syntax.FieldReference(
                "myField", Syntax.VariableReference("myVariable"));

            // Get expression text
            Assert.AreEqual("myVariable.myField", syntax0?.GetSourceText());

            SyntaxNode syntax1 = Syntax.FieldReference(
                "myField", Syntax.FieldReference("myOtherField", Syntax.VariableReference("myVariable")));

            // Get expression text
            Assert.AreEqual("myVariable.myOtherField.myField", syntax1.GetSourceText());
        }
    }
}
