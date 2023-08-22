using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Expressions
{
    [TestClass]
    public class LiteralExpressionTests
    {
        [TestMethod]
        public void Int()
        {
            string input = "5";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("5", context.GetText());
        }

        [TestMethod]
        public void IntUnsigned()
        {
            string input = "5U";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("5U", context.GetText());
        }

        [TestMethod]
        public void IntLong()
        {
            string input = "5L";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("5L", context.GetText());
        }

        [TestMethod]
        public void IntUnsignedLong()
        {
            string input = "5UL";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("5UL", context.GetText());
        }

        [TestMethod]
        public void Decimal()
        {
            string input = "5.538";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("5.538", context.GetText());
        }

        [TestMethod]
        public void DecimalFloat()
        {
            string input = "5.538F";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("5.538F", context.GetText());
        }

        [TestMethod]
        public void DecimalDouble()
        {
            string input = "5.538D";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("5.538D", context.GetText());
        }

        [TestMethod]
        public void Hex_0A()
        {
            string input = "0x00";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("0x00", context.GetText());
        }

        [TestMethod]
        public void Hex_0B()
        {
            string input = "0x0000";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("0x0000", context.GetText());
        }

        [TestMethod]
        public void Hex_0C()
        {
            string input = "0x000000";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("0x000000", context.GetText());
        }

        [TestMethod]
        public void Hex_0D()
        {
            string input = "0x00000000";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("0x00000000", context.GetText());
        }

        [TestMethod]
        public void Hex_A()
        {
            string input = "0x1A";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("0x1A", context.GetText());
        }

        [TestMethod]
        public void Hex_B()
        {
            string input = "0x1A3F";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("0x1A3F", context.GetText());
        }

        [TestMethod]
        public void Hex_C()
        {
            string input = "0x1A3FD6";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("0x1A3FD6", context.GetText());
        }

        [TestMethod]
        public void Hex_D()
        {
            string input = "0x1A3FD63C";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("0x1A3FD63C", context.GetText());
        }

        [TestMethod]
        public void Literal()
        {
            string input = @"""Hello World""";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual(@"""Hello World""", context.GetText());
        }

        [TestMethod]
        public void True()
        {
            string input = "true";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("true", context.GetText());
        }

        [TestMethod]
        public void False()
        {
            string input = "false";
            LumaSharpParser.ExpressionContext context = TestUtils.ParseInputStringExpression(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("false", context.GetText());
        }
    }
}
