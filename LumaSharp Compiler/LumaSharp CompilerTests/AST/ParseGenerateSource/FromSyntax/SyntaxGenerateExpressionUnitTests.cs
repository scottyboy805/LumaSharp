using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Assert.AreEqual("base", syntax.StartToken.Text);
            Assert.AreEqual("base", syntax.EndToken.Text);
        }

        [TestMethod]
        public void GenerateExpression_This()
        {
            SyntaxNode syntax = Syntax.This();

            // Get expression text
            Assert.AreEqual("this", syntax.GetSourceText());
            Assert.AreEqual("this", syntax.StartToken.Text);
            Assert.AreEqual("this", syntax.EndToken.Text);
        }

        [TestMethod]
        public void GenerateExpression_Type()
        {
            SyntaxNode syntax0 = Syntax.TypeOp(
                Syntax.TypeReference(PrimitiveType.I32));

            // Get expression text
            Assert.AreEqual("type(i32)", syntax0.GetSourceText());
            Assert.AreEqual("type", syntax0.StartToken.Text);
            Assert.AreEqual(")", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.TypeOp(
                Syntax.TypeReference("MyType"));

            // Get expression text
            Assert.AreEqual("type(MyType)", syntax1.GetSourceText());
            Assert.AreEqual("type", syntax1.StartToken.Text);
            Assert.AreEqual(")", syntax1.EndToken.Text);
        }

        [TestMethod]
        public void GenerateExpression_Size()
        {
            SyntaxNode syntax0 = Syntax.SizeOp(
                Syntax.TypeReference(PrimitiveType.I32));

            // Get expression text
            Assert.AreEqual("size(i32)", syntax0.GetSourceText());
            Assert.AreEqual("size", syntax0.StartToken.Text);
            Assert.AreEqual(")", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.SizeOp(
                Syntax.TypeReference("MyType"));

            // Get expression text
            Assert.AreEqual("size(MyType)", syntax1.GetSourceText());
            Assert.AreEqual("size", syntax1.StartToken.Text);
            Assert.AreEqual(")", syntax1.EndToken.Text);
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
            Assert.AreEqual(@"""Hello""", syntax2.GetSourceText());

            SyntaxNode syntax3 = Syntax.Literal(false);

            Assert.AreEqual("false", syntax3.GetSourceText());
        }

        [TestMethod]
        public void GenerateExpression_Field()
        {
            SyntaxNode syntax0 = Syntax.FieldReference(
                Syntax.VariableReference("myVariable"), "myField");

            // Get expression text
            Assert.AreEqual("myVariable.myField", syntax0?.GetSourceText());
            Assert.AreEqual("myVariable", syntax0.StartToken.Text);
            Assert.AreEqual("myField", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.FieldReference(Syntax.FieldReference(Syntax.VariableReference("myVariable"), "myOtherField"), "myField");

            // Get expression text
            Assert.AreEqual("myVariable.myOtherField.myField", syntax1.GetSourceText());
            Assert.AreEqual("myVariable", syntax1.StartToken.Text);
            Assert.AreEqual("myField", syntax1.EndToken.Text);
        }

        [TestMethod]
        public void GenerateExpression_Method()
        {
            SyntaxNode syntax0 = Syntax.MethodInvoke(
                Syntax.VariableReference("myVariable"), "myMethod");

            // Get expression text
            Assert.AreEqual("myVariable.myMethod()", syntax0.GetSourceText());
            Assert.AreEqual("myVariable", syntax0.StartToken.Text);
            Assert.AreEqual(")", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.MethodInvoke(
                Syntax.VariableReference("myVariable"), "myMethod", Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false)));

            // Get expression text
            Assert.AreEqual("myVariable.myMethod(5,false)", syntax1.GetSourceText());
            Assert.AreEqual("myVariable", syntax1.StartToken.Text);
            Assert.AreEqual(")", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.MethodInvoke(
                Syntax.VariableReference("myVariable"), "myMethod", 
                Syntax.GenericArgumentList(Syntax.TypeReference(PrimitiveType.I32), Syntax.TypeReference("MyType")));

            // Get expression text
            Assert.AreEqual("myVariable.myMethod<i32,MyType>()", syntax2.GetSourceText());
            Assert.AreEqual("myVariable", syntax2.StartToken.Text);
            Assert.AreEqual(")", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.MethodInvoke(
                Syntax.VariableReference("myVariable"), "myMethod", 
                Syntax.GenericArgumentList(Syntax.TypeReference(PrimitiveType.I32), Syntax.TypeReference("MyType")),
                Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false)));

            // Get expression text
            Assert.AreEqual("myVariable.myMethod<i32,MyType>(5,false)", syntax3.GetSourceText());
            Assert.AreEqual("myVariable", syntax3.StartToken.Text);
            Assert.AreEqual(")", syntax3.EndToken.Text);
        }

        [TestMethod]
        public void GenerateExpression_New()
        {
            SyntaxNode syntax0 = Syntax.New(
                Syntax.TypeReference("MyType"), Syntax.ArgumentList(Syntax.Literal(false)));

            // Get expression text
            Assert.AreEqual("new MyType()", syntax0.GetSourceText());
            Assert.AreEqual("new", syntax0.StartToken.Text);
            Assert.AreEqual(")", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.New(
                Syntax.TypeReference("MyType"));

            // Get expression text
            Assert.AreEqual("MyType()", syntax1.GetSourceText());
            Assert.AreEqual("MyType", syntax1.StartToken.Text);
            Assert.AreEqual(")", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.New(
                Syntax.TypeReference("MyType"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(true)));

            // Get expression text
            Assert.AreEqual("new MyType(5,true)", syntax2.GetSourceText());
            Assert.AreEqual("new", syntax2.StartToken.Text);
            Assert.AreEqual(")", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.New(
                Syntax.TypeReference("MyType"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(true)));

            // Get expression text
            Assert.AreEqual("MyType(5,true)", syntax3.GetSourceText());
            Assert.AreEqual("MyType", syntax3.StartToken.Text);
            Assert.AreEqual(")", syntax3.EndToken.Text);
        }

        [TestMethod]
        public void GenerateExpression_Ternary()
        {
            SyntaxNode syntax0 = Syntax.Ternary(
                Syntax.Literal(true), Syntax.Literal(1), Syntax.Literal(0));

            // Get expression text
            Assert.AreEqual("true?1:0", syntax0.GetSourceText());
            Assert.AreEqual("true", syntax0.StartToken.Text);
            Assert.AreEqual("0", syntax0.EndToken.Text);
        }

        [TestMethod]
        public void GenerateExpression_Binary()
        {
            SyntaxNode syntax0 = Syntax.Binary(
                Syntax.Literal(5), Syntax.Token(SyntaxTokenKind.AddSymbol), Syntax.Literal(10));

            // Get expression text
            Assert.AreEqual("5+10", syntax0.GetSourceText());
            Assert.AreEqual("5", syntax0.StartToken.Text);
            Assert.AreEqual("10", syntax0.EndToken.Text);
        }
    }
}
