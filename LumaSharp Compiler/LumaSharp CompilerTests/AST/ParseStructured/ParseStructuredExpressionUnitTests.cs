using LumaSharp.Compiler.AST;
using LumaSharp.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST.ParseStructured
{
    [TestClass]
    public class ParseStructuredExpressionUnitTests
    {
        [DataTestMethod]
        [DataRow("size(i32)", typeof(TypeReferenceSyntax))]
        [DataRow("size(string)", typeof(TypeReferenceSyntax))]
        [DataRow("size(bool)", typeof(TypeReferenceSyntax))]
        [DataRow("size(MyType<i8>)", typeof(TypeReferenceSyntax))]
        public void StructuredExpression_Size(string input, Type expressionType)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(SizeExpressionSyntax));

            Assert.IsInstanceOfType(((SizeExpressionSyntax)expression).TypeReference, expressionType);
        }

        [DataTestMethod]
        [DataRow("type(i32)", typeof(TypeReferenceSyntax))]
        [DataRow("type(string)", typeof(TypeReferenceSyntax))]
        [DataRow("type(bool)", typeof(TypeReferenceSyntax))]
        [DataRow("type(MyType<i8>)", typeof(TypeReferenceSyntax))]
        public void StructuredExpression_Type(string input, Type expressionType)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(TypeExpressionSyntax));

            Assert.IsInstanceOfType(((TypeExpressionSyntax)expression).TypeReference, expressionType);
        }

        [DataTestMethod]
        [DataRow("i32.myField", typeof(TypeReferenceSyntax))]
        [DataRow("my.Field.Final", typeof(FieldReferenceExpressionSyntax))]
        [DataRow("my.Method(a).Final", typeof(MethodInvokeExpressionSyntax))]
        [DataRow("my.Array[0].Final", typeof(ArrayIndexExpressionSyntax))]
        [DataRow(@"""Hello World"".Length", typeof(LiteralExpressionSyntax))]
        [DataRow("1234.Final", typeof(LiteralExpressionSyntax))]
        [DataRow("(43 + 24).Final", typeof(BinaryExpressionSyntax))]
        [DataRow("new MyType().Field", typeof(NewExpressionSyntax))]
        [DataRow("ident.Final", typeof(VariableReferenceExpressionSyntax))]
        [DataRow("MyUserType<i32, string>.Final", typeof(TypeReferenceSyntax))]     // Not working
        [DataRow("this.Final", typeof(ThisExpressionSyntax))]
        [DataRow("base.Final", typeof(BaseExpressionSyntax))]
        public void StructuredExpression_Field(string input, Type expressionType)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(FieldReferenceExpressionSyntax));

            Assert.IsInstanceOfType(((FieldReferenceExpressionSyntax)expression).AccessExpression, expressionType);
        }

        [DataTestMethod]
        [TestMethod]
        [DataRow("i32.myMethod()", typeof(TypeReferenceSyntax))]
        [DataRow("my.Field.Final()", typeof(FieldReferenceExpressionSyntax))]
        [DataRow("my.Method(a).Final()", typeof(MethodInvokeExpressionSyntax))]
        [DataRow("my.Array[0].Final(123)", typeof(ArrayIndexExpressionSyntax))]
        [DataRow(@"""Hello World"".Length()", typeof(LiteralExpressionSyntax))]
        [DataRow("1234.Final()", typeof(LiteralExpressionSyntax))]
        [DataRow("(43 + 24).Final()", typeof(BinaryExpressionSyntax))]
        [DataRow("new MyType().Field()", typeof(NewExpressionSyntax))]
        [DataRow("ident.Final(true)", typeof(VariableReferenceExpressionSyntax))]
        [DataRow("MyUserType<i32, string>.Final(1)", typeof(TypeReferenceSyntax))]
        [DataRow("this.Final(1, true)", typeof(ThisExpressionSyntax))]
        [DataRow("base.Final(false, true)", typeof(BaseExpressionSyntax))]
        public void StructuredExpression_Method(string input, Type expressionType)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(MethodInvokeExpressionSyntax));

            Assert.IsInstanceOfType(((MethodInvokeExpressionSyntax)expression).AccessExpression, expressionType);
        }

        [DataTestMethod]
        [DataRow("my.Field[0]", typeof(FieldReferenceExpressionSyntax))]
        [DataRow("my.Method(a)[0]", typeof(MethodInvokeExpressionSyntax))]
        [DataRow("my.Array[0][0, 1]", typeof(ArrayIndexExpressionSyntax))]
        [DataRow(@"""Hello World""[0, 1]", typeof(LiteralExpressionSyntax))]
        [DataRow("1234[1]", typeof(LiteralExpressionSyntax))]
        [DataRow("(43 + 24)[1]", typeof(BinaryExpressionSyntax))]
        [DataRow("new MyType()[1]", typeof(NewExpressionSyntax))]
        [DataRow("ident[0, 1]", typeof(VariableReferenceExpressionSyntax))]
        [DataRow("this[0, 1]", typeof(ThisExpressionSyntax))]
        [DataRow("base[0]", typeof(BaseExpressionSyntax))]
        public void StructuredExpression_Array(string input, Type expressionType)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ArrayIndexExpressionSyntax));

            Assert.IsInstanceOfType(((ArrayIndexExpressionSyntax)expression).AccessExpression, expressionType);
        }
    }
}
