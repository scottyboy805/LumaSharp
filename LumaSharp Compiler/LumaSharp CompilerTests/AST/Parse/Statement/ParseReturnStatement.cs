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
    }
}
