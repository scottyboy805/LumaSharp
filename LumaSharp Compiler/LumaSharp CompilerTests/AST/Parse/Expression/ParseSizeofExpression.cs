using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseSizeofExpression
    {
        // Methods
        [DataTestMethod]
        [DataRow("sizeof(i32)")]
        [DataRow("sizeof ( i32 )")]
        [DataRow("sizeof\t(\ti32\t)\t")]
        [DataRow("sizeof(SomeType)")]
        public void ParseAsSizeofExpression(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(SizeofExpressionSyntax));
        }
    }
}
