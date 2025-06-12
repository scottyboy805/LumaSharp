using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace CompilerTests.Token
{
    [TestClass]
    public class TokenParserTests
    {
        // Methods
        //[DataTestMethod]
        //[DataRow("//", "")]
        //[DataRow("// My Comment", " My Comment")]
        //[DataRow(" \t//Some comment", "Some comment")]
        //public void TestLineComment(string source, string expectedComment)
        //{
        //    // Create string reader
        //    StringReader stringReader = new StringReader(source);

        //    // Create parser
        //    TokenParser tokenParser = new(new TextView(stringReader), null);

        //    // Get tokens
        //    var tokens = tokenParser.ToList();

        //    // Check for tokens
        //    Assert.IsTrue(tokens.Any(t => t.Kind == SyntaxTokenKind.LineComment));
        //    Assert.AreEqual(1, tokens.Count(t => t.Kind == SyntaxTokenKind.LineComment));
        //    Assert.AreEqual(expectedComment, tokens.First(t => t.Kind == SyntaxTokenKind.CommentText).Text);
        //}

        //[DataTestMethod]
        //[DataRow("/**/", "")]
        //[DataRow("/*My Comment*/", "My Comment")]
        //[DataRow(" \t/* Some comment */", " Some comment ")]
        //public void TestBlockComment(string source, string expectedComment)
        //{
        //    // Create string reader
        //    StringReader stringReader = new StringReader(source);

        //    // Create parser
        //    TokenParser tokenParser = new(new TextView(stringReader));

        //    // Get tokens
        //    var tokens = tokenParser.ToList();

        //    // Check for tokens
        //    Assert.IsTrue(tokens.Any(t => t.Kind == SyntaxTokenKind.BlockCommentStart));
        //    Assert.IsTrue(tokens.Any(t => t.Kind == SyntaxTokenKind.BlockCommentEnd));
        //    Assert.AreEqual(1, tokens.Count(t => t.Kind == SyntaxTokenKind.BlockCommentStart));
        //    Assert.AreEqual(1, tokens.Count(t => t.Kind == SyntaxTokenKind.BlockCommentEnd));
        //    Assert.AreEqual(expectedComment, tokens.First(t => t.Kind == SyntaxTokenKind.CommentText).Text);
        //}

        [DataTestMethod]
        [DataRow(@"""My Quote""", "My Quote")]
        public void TestLiteralString(string source, string expectedLiteral)
        {
            // Create string reader
            StringReader stringReader = new StringReader(source);

            // Create parser
            TokenParser tokenParser = new(new TextView(stringReader), null);

            // Get tokens
            var tokens = tokenParser.ToList();

            // Check for tokens
            Assert.IsTrue(tokens.Any(t => t.Kind == SyntaxTokenKind.Literal));
            Assert.AreEqual(expectedLiteral, tokens.First(t => t.Kind == SyntaxTokenKind.Literal).Text);
        }

        [DataTestMethod]
        public void TestKeywords()
        {
            // Process all possible keywords
            foreach(string keyword in SyntaxToken.GetKeywords().Select(SyntaxToken.GetText))
            {
                // Create string reader
                StringReader stringReader = new StringReader(keyword);

                // Create parser
                TokenParser tokenParser = new(new TextView(stringReader), null);

                // Get tokens
                var tokens = tokenParser.ToList();

                // Check for tokens
                Assert.IsTrue(tokens.Any(t => t.IsKeyword), keyword);
                Assert.AreEqual(keyword, tokens.First(t => t.IsKeyword).Text, keyword);
            }
        }

        [DataTestMethod]
        public void TestSymbols()
        {
            // Process all possible keywords
            foreach (string symbol in SyntaxToken.GetSymbols()
                .Select(SyntaxToken.GetText))
            {
                // Create string reader
                StringReader stringReader = new StringReader(symbol);

                // Create parser
                TokenParser tokenParser = new(new TextView(stringReader), null);

                // Get tokens
                var tokens = tokenParser.ToList();

                // Check for tokens
                Assert.IsTrue(tokens.Any(t => t.IsSymbol), symbol);
                Assert.AreEqual(symbol, tokens.First(t => t.IsSymbol).Text, symbol);
            }
        }
    }
}
