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
    }
}
