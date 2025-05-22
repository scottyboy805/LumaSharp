using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Statement
{
    [TestClass]
    public class ParseContinueStatement
    {
        // Methods
        [DataTestMethod]
        [DataRow("continue;")]
        [DataRow("continue ;")]
        public void ParseAsContinueStatement(string input)
        {
            // Try to parse the tree
            StatementSyntax statement = TestUtils.ParseInputStringStatement(input);

            Assert.IsNotNull(statement);
            Assert.IsInstanceOfType(statement, typeof(ContinueStatementSyntax));
        }

        [DataTestMethod]
        [DataRow("continue;")]
        public void ParseContinueNode(string input)
        {
            // Try to parse the tree
            ContinueStatementSyntax statement = TestUtils.ParseInputStringStatement(input,
                p => p.ParseContinueStatement() as ContinueStatementSyntax);

            Assert.IsNotNull(statement);
            Assert.AreEqual(SyntaxTokenKind.ContinueKeyword, statement.Keyword.Kind);
            Assert.AreEqual(SyntaxTokenKind.SemicolonSymbol, statement.Semicolon.Kind);
            Assert.AreEqual(SyntaxTokenKind.ContinueKeyword, statement.StartToken.Kind);
            Assert.AreEqual(SyntaxTokenKind.SemicolonSymbol, statement.EndToken.Kind);
            Assert.IsFalse(statement.Descendants.Any());
        }
    }
}
