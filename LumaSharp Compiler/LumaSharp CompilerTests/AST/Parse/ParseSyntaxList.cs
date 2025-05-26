using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse
{
    [TestClass]
    public class ParseSyntaxList
    {
        [DataTestMethod]
        [DataRow("1")]
        [DataRow("1,2")]
        [DataRow("1,2,3")]
        public void ParseAsExpressionCommaTokenList(string input)
        {
            // Try to parse the tree
            SeparatedSyntaxList<ExpressionSyntax> list = TestUtils.ParseSeparatedExpressionList(input, SyntaxTokenKind.CommaSymbol);

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count >= 1);
        }
    }
}
