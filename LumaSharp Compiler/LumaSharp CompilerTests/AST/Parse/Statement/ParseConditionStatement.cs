using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Statement
{
    [TestClass]
    public class ParseConditionStatement
    {
        // Methods
        [DataTestMethod]
        // If
        [DataRow("if true;")]
        [DataRow("if false{}")]
        [DataRow("if a + b;")]
        [DataRow("if c+ d{}")]
        [DataRow("if (1);")]
        [DataRow("if (2){}")]
        [DataRow("if (aa + bb);")]
        [DataRow("if( cc+ dd){}")]

        // If else
        [DataRow("if true; else;")]
        [DataRow("if false{}else{}")]
        [DataRow("if a + b;else;")]
        [DataRow("if c+ d{}else;")]
        [DataRow("if (1);else;")]
        [DataRow("if (2){}else;")]
        [DataRow("if (aa + bb);else;")]
        [DataRow("if( cc+ dd){}else;")]
        public void ParseAsConditionStatement(string input)
        {
            // Try to parse the tree
            StatementSyntax statement = TestUtils.ParseInputStringStatement(input);

            Assert.IsNotNull(statement);
            Assert.IsInstanceOfType(statement, typeof(ConditionStatementSyntax));
        }
    }
}
