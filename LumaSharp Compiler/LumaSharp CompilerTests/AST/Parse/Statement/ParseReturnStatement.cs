using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Statement
{
    [TestClass]
    public class ParseReturnStatement
    {
        // Methods
        [DataTestMethod]
        [DataRow("return;")]
        [DataRow("return val;")]
        [DataRow("return a,b;")]
        [DataRow("return a,b,c;")]
        public void ParseAsReturnStatement(string input)
        {
            // Try to parse the tree
            StatementSyntax statement = TestUtils.ParseInputStringStatement(input);

            Assert.IsNotNull(statement);
            Assert.IsInstanceOfType(statement, typeof(ReturnStatementSyntax));
        }

        [DataTestMethod]
        [DataRow("return;")]
        public void ParseReturnNode(string input)
        {
            // Try to parse the tree
            ReturnStatementSyntax statement = TestUtils.ParseInputStringStatement(input,
                p => p.ParseReturnStatement() as ReturnStatementSyntax);

            Assert.IsNotNull(statement);
            Assert.AreEqual(SyntaxTokenKind.ReturnKeyword, statement.Keyword.Kind);
            Assert.AreEqual(SyntaxTokenKind.SemicolonSymbol, statement.Semicolon.Kind);
            Assert.AreEqual(SyntaxTokenKind.ReturnKeyword, statement.StartToken.Kind);
            Assert.AreEqual(SyntaxTokenKind.SemicolonSymbol, statement.EndToken.Kind);
            Assert.IsFalse(statement.Descendants.Any());
        }

        [DataTestMethod]
        [DataRow("return true;")]
        public void ParseReturnSingleNode(string input)
        {
            // Try to parse the tree
            ReturnStatementSyntax statement = TestUtils.ParseInputStringStatement(input,
                p => p.ParseReturnStatement() as ReturnStatementSyntax);

            Assert.IsNotNull(statement);
            Assert.AreEqual(SyntaxTokenKind.ReturnKeyword, statement.Keyword.Kind);
            Assert.AreEqual(SyntaxTokenKind.SemicolonSymbol, statement.Semicolon.Kind);
            Assert.AreEqual(SyntaxTokenKind.ReturnKeyword, statement.StartToken.Kind);
            Assert.AreEqual(SyntaxTokenKind.SemicolonSymbol, statement.EndToken.Kind);
            Assert.AreEqual(1, statement.Descendants.Count());
        }

        [DataTestMethod]
        [DataRow("return true, false;")]
        public void ParseReturnMultipleNode(string input)
        {
            // Try to parse the tree
            ReturnStatementSyntax statement = TestUtils.ParseInputStringStatement(input,
                p => p.ParseReturnStatement() as ReturnStatementSyntax);

            Assert.IsNotNull(statement);
            Assert.AreEqual(SyntaxTokenKind.ReturnKeyword, statement.Keyword.Kind);
            Assert.AreEqual(SyntaxTokenKind.SemicolonSymbol, statement.Semicolon.Kind);
            Assert.AreEqual(SyntaxTokenKind.ReturnKeyword, statement.StartToken.Kind);
            Assert.AreEqual(SyntaxTokenKind.SemicolonSymbol, statement.EndToken.Kind);
            Assert.AreEqual(1, statement.Descendants.Count());  // This is a separated list no matter how many return values
        }
    }
}
