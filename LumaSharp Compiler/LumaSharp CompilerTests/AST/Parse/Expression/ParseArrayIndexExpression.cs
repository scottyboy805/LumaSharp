using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseArrayIndexExpression
    {
        // Methods
        [DataTestMethod]
        [DataRow("someVariable[5]")]
        [DataRow("parameter[6,2]")]
        [DataRow(@"""Literal""[5]")]
        [DataRow("some.field[5]")]
        [DataRow("some.Method()[5]")]
        [DataRow("(someVariable)[5]")]
        [DataRow("(someVariable + someOther)[5]")]
        [DataRow("var[5][3]")]
        public void ParseAsArrayIndexExpression(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(IndexExpressionSyntax));
        }
    }
}
