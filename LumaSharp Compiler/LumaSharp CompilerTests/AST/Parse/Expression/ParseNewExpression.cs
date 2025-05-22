using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseNewExpression
    {
        // Methods
        [DataTestMethod]
        [DataRow("new i32()")]
        [DataRow("new MyType()")]
        [DataRow("new myGenericType<i32>()")]
        [DataRow("new myGenericType<i32, SomeOtherType<i8>, string>()")]
        public void ParseAsNewExpression(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(NewExpressionSyntax));
        }
    }
}
