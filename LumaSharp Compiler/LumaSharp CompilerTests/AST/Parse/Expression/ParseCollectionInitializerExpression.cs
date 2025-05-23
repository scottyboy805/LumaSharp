using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseCollectionInitializerExpression
    {
        // Methods
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{a}")]
        [DataRow("{a,b}")]
        [DataRow("{1,2,3,4,5}")]
        public void ParseAsCollectionInitializerExpression(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(CollectionInitializerExpressionSyntax));
        }
    }
}
