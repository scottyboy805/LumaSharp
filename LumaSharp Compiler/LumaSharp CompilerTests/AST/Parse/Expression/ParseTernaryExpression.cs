using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseTernaryExpression
    {
        // Methods
        [DataTestMethod]
        [DataRow("true ? 1 : 0")]
        [DataRow("1 ? varA : varB")]
        public void ParseAsTernaryExpression(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(TernaryExpressionSyntax));
        }
    }
}
