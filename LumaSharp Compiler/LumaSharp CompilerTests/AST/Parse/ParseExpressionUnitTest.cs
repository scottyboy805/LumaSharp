using LumaSharp.Compiler;
using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST.Parse
{
    [TestClass]
    public class ParseExpressionUnitTest
    {
        //[DataTestMethod]
        //[DataRow(" ")]
        //public void CompilationUnit_Empty(string input)
        //{
        //    // Try to parse the tree
        //    ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

        //    Assert.IsNull(expression);
        //}

        [DataTestMethod]
        [DataRow("base")]
        public void Expression_Base(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(BaseExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("this")]
        public void Expression_This(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ThisExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("type(i32)")]
        public void Expression_Type(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(TypeofExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("size(i32)")]
        public void Expression_Size(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(SizeofExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("someVariable")]
        [DataRow("parameter")]
        public void Expression_Variable(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(VariableReferenceExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("i32.maxSize")]
        [DataRow("string.terminatingCharacter")]
        [DataRow("someval.terminatingCharacter")]
        public void Expression_Field(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(FieldReferenceExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("i32.Clamp()")]
        [DataRow("string.Clamp()")]
        [DataRow("i32.Clamp(123)")]
        [DataRow("string.Clamp(123, someVar)")]
        [DataRow("someVariable.Clamp()")]
        [DataRow("SomeType<i32>.Clamp()", "Type")]
        public void Expression_Method(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(MethodInvokeExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("new i32()")]
        [DataRow("new MyType()")]
        [DataRow("new myGenericType<i32>()")]
        [DataRow("new myGenericType<i32, string, SomeOtherType<i8>>()")]
        public void Expression_New(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(NewExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("1234")]
        [DataRow("1234L")]
        [DataRow("1234U")]
        [DataRow("1234UL")]
        [DataRow("123.456")]
        [DataRow("123.456F")]
        [DataRow("123.456D")]
        [DataRow("0xA0")]
        [DataRow("0xBA34")]
        [DataRow("0xD2F4A1")]
        [DataRow("0xF954E1CB")]
        [DataRow(@"""Hello World""")]
        [DataRow("true")]
        [DataRow("false")]
        [DataRow("null")]
        public void Expression_Literal(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(LiteralExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("true ? 1 : 0")]
        [DataRow("1 ? varA : varB")]
        public void Expression_Ternary(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(TernaryExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("4 + 2")]
        [DataRow("a - b")]
        [DataRow("true % false")]
        [DataRow("45.2F - 84.2FD")]
        public void Expression_Binary(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(BinaryExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("someVariable[5]")]
        [DataRow("parameter[6,2]")]
        [DataRow(@"""Literal""[5]")]
        [DataRow("some.field[5]")]
        [DataRow("some.Method()[5]")]
        [DataRow("(someVariable)[5]")]
        [DataRow("(someVariable + someOther)[5]")]
        [DataRow("var[5][3]")]
        public void Expression_ArrayIndex(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ArrayIndexExpressionSyntax));
        }
    }
}
