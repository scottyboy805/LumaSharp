using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Statement
{
    [TestClass]
    public class ParseBreakStatement
    {
        // Methods
        [DataTestMethod]
        [DataRow("break;")]
        [DataRow("break ;")]
        public void ParseAsBreakStatement(string input)
        {
            // Try to parse the tree
            StatementSyntax statement = TestUtils.ParseInputStringStatement(input);

            Assert.IsNotNull(statement);
            Assert.IsInstanceOfType(statement, typeof(BreakStatementSyntax));
        }

        [DataTestMethod]
        [DataRow("break;")]
        public void ParseBreakNode(string input)
        {
            // Try to parse the tree
            BreakStatementSyntax statement = TestUtils.ParseInputStringStatement(input, 
                p => p.ParseBreakStatement() as BreakStatementSyntax);

            Assert.IsNotNull(statement);
            Assert.AreEqual(SyntaxTokenKind.BreakKeyword, statement.Keyword.Kind);
            Assert.AreEqual(SyntaxTokenKind.SemicolonSymbol, statement.Semicolon.Kind);
            Assert.AreEqual(SyntaxTokenKind.BreakKeyword, statement.StartToken.Kind);
            Assert.AreEqual(SyntaxTokenKind.SemicolonSymbol, statement.EndToken.Kind);
            Assert.IsFalse(statement.Descendants.Any());
        }
    }
}
