using LumaSharp.Compiler.AST;
using LumaSharp.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST.ParseStructured
{
    [TestClass]
    public class ParseStructuredExpressionUnitTests
    {
        [DataTestMethod]
        [DataRow("sizeof(i32)")]
        [DataRow("sizeof(string)")]
        [DataRow("sizeof(bool)")]
        [DataRow("sizeof(MyType<i8>)")]
        public void StructuredExpression_Sizeof(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(SizeofExpressionSyntax));
            Assert.IsNotNull(((SizeofExpressionSyntax)expression).TypeReference);
        }

        [DataTestMethod]
        [DataRow("typeof(i32)")]
        [DataRow("typeof(string)")]
        [DataRow("typeof(bool)")]
        [DataRow("typeof(MyType<i8>)")]
        public void StructuredExpression_Typeof(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(TypeofExpressionSyntax));
            Assert.IsNotNull(((TypeofExpressionSyntax)expression).TypeReference);
        }

        [DataTestMethod]
        [DataRow("i32.myField", typeof(TypeReferenceSyntax))]
        [DataRow("my.Field.Final", typeof(MemberAccessExpressionSyntax))]
        [DataRow("my.Method(a).Final", typeof(MethodInvokeExpressionSyntax))]
        [DataRow("my.Array[0].Final", typeof(IndexExpressionSyntax))]
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
            Assert.IsInstanceOfType(expression, typeof(MemberAccessExpressionSyntax));

            Assert.IsInstanceOfType(((MemberAccessExpressionSyntax)expression).AccessExpression, expressionType);
        }

        [DataTestMethod]
        [TestMethod]
        [DataRow("i32.myMethod()", typeof(TypeReferenceSyntax))]
        [DataRow("my.Field.Final()", typeof(MemberAccessExpressionSyntax))]
        [DataRow("my.Method(a).Final()", typeof(MethodInvokeExpressionSyntax))]
        [DataRow("my.Array[0].Final(123)", typeof(IndexExpressionSyntax))]
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
        [DataRow("my.Field[0]", typeof(MemberAccessExpressionSyntax))]
        [DataRow("my.Method(a)[0]", typeof(MethodInvokeExpressionSyntax))]
        [DataRow("my.Array[0][0, 1]", typeof(IndexExpressionSyntax))]
        [DataRow(@"""Hello World""[0, 1]", typeof(LiteralExpressionSyntax))]
        [DataRow("1234[1]", typeof(LiteralExpressionSyntax))]
        [DataRow("(43 + 24)[1]", typeof(ParenthesizedExpressionSyntax))]
        [DataRow("new MyType()[1]", typeof(NewExpressionSyntax))]
        [DataRow("ident[0, 1]", typeof(VariableReferenceExpressionSyntax))]
        [DataRow("this[0, 1]", typeof(ThisExpressionSyntax))]
        [DataRow("base[0]", typeof(BaseExpressionSyntax))]
        public void StructuredExpression_Array(string input, Type expressionType)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(IndexExpressionSyntax));

            Assert.IsInstanceOfType(((IndexExpressionSyntax)expression).AccessExpression, expressionType);
        }
    }
}
