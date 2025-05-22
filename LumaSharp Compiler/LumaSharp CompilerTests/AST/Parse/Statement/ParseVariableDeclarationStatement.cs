using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Statement
{
    [TestClass]
    public class ParseVariableDeclarationStatement
    {
        // Methods
        [DataTestMethod]
        [DataRow("i32 var;")]
        [DataRow("string value;")]
        [DataRow("f32 val1, val2;")]
        [DataRow("char a,b,c;")]
        [DataRow("i32 varA = 123;")]
        [DataRow("string value = null;")]
        [DataRow("f32 val1, val2 = 45.5,23.8;")]
        [DataRow("char a,b,c = 1, 2, 3;")]
        public void ParseAsVariableDeclarationStatement(string input)
        {
            // Try to parse the tree
            StatementSyntax statement = TestUtils.ParseInputStringStatement(input);

            Assert.IsNotNull(statement);
            Assert.IsInstanceOfType(statement, typeof(VariableDeclarationStatementSyntax));
        }
    }
}
