using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseLiteralExpression
    {
        // Methods
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
        public void ParseAsLiteralExpression(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(LiteralExpressionSyntax));
        }
    }
}
