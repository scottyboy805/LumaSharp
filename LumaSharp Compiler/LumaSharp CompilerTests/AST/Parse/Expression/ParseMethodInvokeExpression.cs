using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseMethodInvokeExpression
    {
        // Methods
        [DataTestMethod]
        [DataRow("i32.Clamp()")]
        [DataRow("string.Clamp()")]
        [DataRow("i32.Clamp(123)")]
        [DataRow("string.Clamp(123, someVar)")]
        [DataRow("someVariable.Clamp()")]
        [DataRow("SomeType<i32>.Clamp()")]
        [DataRow("base.Clamp()")]
        [DataRow("this.Clamp()")]
        [DataRow("some[0].Clamp()")]
        [DataRow("150.Clamp()")]
        public void ParseAsMethodInvokeExpression(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(MethodInvokeExpressionSyntax));
        }
    }
}
