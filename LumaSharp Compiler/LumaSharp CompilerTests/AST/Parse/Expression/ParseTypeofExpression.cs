using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseTypeofExpression
    {
        // Methods
        [DataTestMethod]
        [DataRow("typeof(i32)")]
        [DataRow("typeof ( i32 )")]
        [DataRow("typeof\t(\ti32\t)\t")]
        [DataRow("typeof(SomeType)")]
        public void ParseAsTypeofExpression(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(TypeofExpressionSyntax));
        }
    }
}
