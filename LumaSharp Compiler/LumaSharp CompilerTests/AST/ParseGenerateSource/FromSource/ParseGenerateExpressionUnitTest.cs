using LumaSharp.Compiler.AST;
using LumaSharp.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.ParseGenerateSource.FromSource
{
    [TestClass]
    public class ParseGenerateExpressionUnitTest
    {
        [DataTestMethod]
        [DataRow("base")]
        [DataRow(" base")]
        [DataRow("  base  ")]
        [DataRow(" \tbase\t ")]
        [DataRow(" \nbase\n ")]
        public void GenerateExpression_Base(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(BaseExpressionSyntax));

            // Get expression text
            Assert.AreEqual(input, expression.GetSourceText());
        }

        [DataTestMethod]
        [DataRow("this")]
        [DataRow(" this ")]
        [DataRow("  this  ")]
        [DataRow(" \tthis\t ")]
        [DataRow(" \nthis\n ")]
        public void GenerateExpression_This(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ThisExpressionSyntax));

            // Get expression text
            Assert.AreEqual(input, expression.GetSourceText());
        }

        [DataTestMethod]
        [DataRow("typeof(i32)")]
        [DataRow("typeof ( i32 ) ")]
        [DataRow("typeof  (  i32  )  ")]
        [DataRow("typeof \t( \ti32 \t)\t ")]
        [DataRow("typeof \n( \ni32 \n)\n ")]
        [DataRow("typeof \n\t( \n\ti32 \n\t)\n\t ")]
        [DataRow("typeof(\t\t\ti32)\t\t\t")]
        [DataRow("typeof(\n\n\ni32)\n\n\n")]
        public void GenerateExpression_Typeof(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(TypeofExpressionSyntax));

            // Get expression text
            Assert.AreEqual(input, expression.GetSourceText());
        }
    }
}
