using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseBinaryExpression
    {
        // Methods
        [DataTestMethod]
        [DataRow("2 + 1")]
        [DataRow("a - b")]
        [DataRow("base * this")]
        [DataRow("4[0] / 2")]
        [DataRow("c == d")]
        [DataRow("e != f")]
        [DataRow("8 || 4")]
        [DataRow("16 && 8")]
        [DataRow("32 | 16")]
        [DataRow("64 & 32")]
        [DataRow("128 ^ 64")]
        [DataRow("true % false")]
        [DataRow("45.2F - 84.2D")]
        [DataRow("(5 - 2) / absoluteValue")]
        public void ParseAsBinaryExpression(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(BinaryExpressionSyntax));
        }
    }
}
