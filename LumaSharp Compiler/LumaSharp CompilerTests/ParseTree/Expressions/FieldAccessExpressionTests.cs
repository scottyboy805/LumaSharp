using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Expressions
{
    [TestClass]
    public class FieldAccessExpressionTests
    {
        [TestMethod]
        public void Field()
        {
            string input = "MyIdentifier.myField";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);
            LumaSharpParser.FieldAccessExpressionContext field = context.fieldAccessExpression();

            Console.WriteLine(context.GetChild(0).GetType());
            Console.WriteLine(context.GetType());
            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(field);
            Assert.AreEqual("MyIdentifier", context.GetChild(0).GetText());
            Assert.AreEqual("myField", field.GetChild(1).GetText());
        }

        [TestMethod]
        public void FieldNested()
        {
            string input = "MyIdentifier.myField1.myField2";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);
            LumaSharpParser.FieldAccessExpressionContext field = context.GetChild<LumaSharpParser.FieldAccessExpressionContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(field);
            Assert.AreEqual("MyIdentifier.myField1", context.GetChild(0).GetText());
            Assert.AreEqual("myField2", field.GetChild(1).GetText());
            //Assert.AreEqual("myField2", field.GetChild(3).GetText());
        }
    }
}
