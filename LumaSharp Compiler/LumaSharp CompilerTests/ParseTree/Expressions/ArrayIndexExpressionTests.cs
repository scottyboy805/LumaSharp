using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Expressions
{
    [TestClass]
    public class ArrayIndexExpressionTests
    {
        [TestMethod]
        public void Array()
        {
            string input = "MyArray[0]";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);
            LumaSharpParser.IndexExpressionContext array = context.GetChild<LumaSharpParser.IndexExpressionContext>(0);
            
            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(array);
            Assert.AreEqual("MyArray", context.GetChild(0).GetText());
            Assert.AreEqual("0", array.GetChild(1).GetText());
        }
    }
}
