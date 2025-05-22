using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Statement
{
    [TestClass]
    public class ParseForStatement
    {
        // Methods
        [DataTestMethod]
        [DataRow("for ;;;")]
        [DataRow("for;;{}")]
        [DataRow("for i32 i;;{}")]
        [DataRow("for i32 val = 0;;{}")]
        [DataRow("for;i<10;{}")]
        [DataRow("for;;i += 5{}")]
        [DataRow("for i32 i = 0; i < 10 ; i++ {}")]
        public void ParseAsForStatement(string input)
        {
            // Try to parse the tree
            StatementSyntax statement = TestUtils.ParseInputStringStatement(input);

            Assert.IsNotNull(statement);
            Assert.IsInstanceOfType(statement, typeof(ForStatementSyntax));
        }
    }
}
