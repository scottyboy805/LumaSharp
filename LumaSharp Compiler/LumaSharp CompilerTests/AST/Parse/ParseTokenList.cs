using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse
{
    [TestClass]
    public class ParseTokenList
    {
        // Methods
        [TestMethod]
        public void ParseAsNullTokenList()
        {
            // Try to parse the tree
            SeparatedTokenList list = TestUtils.ParseSeparatedTokenList("", SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier, false);

            Assert.IsNull(list);
        }

        [DataTestMethod]
        [DataRow("value")]
        public void ParseAsNullMatchTokenList(string input)
        {
            // Try to parse the tree
            SeparatedTokenList list = TestUtils.ParseSeparatedTokenList(input, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier, true);

            Assert.IsNull(list);
        }

        [DataTestMethod]
        [DataRow("value")]
        [DataRow("value,other")]
        [DataRow("value,other,something")]
        public void ParseAsIdentifierCommaTokenList(string input)
        {
            // Try to parse the tree
            SeparatedTokenList list = TestUtils.ParseSeparatedTokenList(input, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier, false);

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count >= 1);
        }

        [DataTestMethod]
        [DataRow("value,")]
        [DataRow("value,other,")]
        [DataRow("value,other,something,")]
        public void ParseAsIdentifierCommaTokenTrailingList(string input)
        {
            // Try to parse the tree
            SeparatedTokenList list = TestUtils.ParseSeparatedTokenList(input, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier, false);

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count >= 1);
        }
    }
}
