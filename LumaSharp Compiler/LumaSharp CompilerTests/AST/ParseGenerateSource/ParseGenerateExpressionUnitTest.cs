using LumaSharp_Compiler.Syntax;
using LumaSharp_Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST.ParseGenerateSource
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
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

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
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ThisExpressionSyntax));

            // Get expression text
            Assert.AreEqual(input, expression.GetSourceText());
        }

        [DataTestMethod]
        [DataRow("type(i32)")]
        [DataRow("type ( i32 ) ")]
        [DataRow("type  (  i32  )  ")]
        [DataRow("type \t( \ti32 \t)\t ")]
        [DataRow("type \n( \ni32 \n)\n ")]
        [DataRow("type \n\t( \n\ti32 \n\t)\n\t ")]
        [DataRow("type(\t\t\ti32)\t\t\t")]
        [DataRow("type(\n\n\ni32)\n\n\n")]
        public void GenerateExpression_Type(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(TypeExpressionSyntax));

            // Get expression text
            Assert.AreEqual(input, expression.GetSourceText());
        }
    }
}
