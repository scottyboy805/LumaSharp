using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Statement
{
    [TestClass]
    public class ParseForeachStatement
    {
        // Methods
        [DataTestMethod]
        [DataRow("for value in expression;")]
        [DataRow("for a in value{}")]
        [DataRow("for i32 value in expression;")]
        [DataRow("for string a in value{}")]
        public void ParseAsForStatement(string input)
        {
            // Try to parse the tree
            StatementSyntax statement = TestUtils.ParseInputStringStatement(input);

            Assert.IsNotNull(statement);
            Assert.IsInstanceOfType(statement, typeof(ForeachStatementSyntax));
        }
    }
}
