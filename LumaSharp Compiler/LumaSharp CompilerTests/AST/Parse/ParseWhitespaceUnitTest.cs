using LumaSharp.Compiler.AST;
using LumaSharp.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST.Parse
{
    [TestClass]
    public class ParseWhitespaceUnitTest
    {
        //[DataTestMethod]
        //[DataRow(" base", " ")]
        //[DataRow("  base", "  ")]
        //[DataRow(" \nbase", " \n")]
        //[DataRow(" \tbase", " \t")]
        //[DataRow(" \n\tbase", " \n\t")]
        //public void WhitespaceLeft(string input, string whitespace)
        //{
        //    // Try to parse the tree
        //    ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

        //    Assert.IsNotNull(expression);
        //    Assert.IsInstanceOfType(expression, typeof(BaseExpressionSyntax));
        //    Assert.AreEqual(whitespace, ((BaseExpressionSyntax)expression).Keyword.LeadingWhitespace);
        //}

        //[DataTestMethod]
        //[DataRow("base ", " ")]
        //[DataRow("base  ", "  ")]
        //[DataRow("base\t ", "\t ")]
        //[DataRow("base\n ", "\n ")]
        //[DataRow("base\n\t ", "\n\t ")]
        //public void WhitespaceRight(string input, string whitespace)
        //{
        //    // Try to parse the tree
        //    ExpressionSyntax expression = SyntaxTree.ParseExpression(InputSource.FromSourceText(input));

        //    Assert.IsNotNull(expression);
        //    Assert.IsInstanceOfType(expression, typeof(BaseExpressionSyntax));
        //    Assert.AreEqual(whitespace, ((BaseExpressionSyntax)expression).Keyword.LeadingWhitespace);
        //}
    }
}
