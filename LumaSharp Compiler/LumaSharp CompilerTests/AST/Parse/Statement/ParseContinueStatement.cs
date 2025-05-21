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
    }
}
