using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Statements
{
    [TestClass]
    public class AssignStatementTests
    {
        [TestMethod]
        public void AssignVariable_Int()
        {
            string input = "MyVariable = 5;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.AssignStatementContext assign = context.GetChild<LumaSharpParser.AssignStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyVariable", assign.GetChild(0).GetText());
            Assert.AreEqual("5", assign.GetChild(2).GetText());
        }

        [TestMethod]
        public void AssignVariable_String()
        {
            string input = @"MyVariable = ""Hello"";";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.AssignStatementContext assign = context.GetChild<LumaSharpParser.AssignStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyVariable", assign.GetChild(0).GetText());
            Assert.AreEqual(@"""Hello""", assign.GetChild(2).GetText());
        }

        [TestMethod]
        public void AssignAddVariable_Int()
        {
            string input = "MyVariable += 15;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.AssignStatementContext assign = context.GetChild<LumaSharpParser.AssignStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyVariable", assign.GetChild(0).GetText());
            Assert.AreEqual("15", assign.GetChild(2).GetText());
        }
    }
}
