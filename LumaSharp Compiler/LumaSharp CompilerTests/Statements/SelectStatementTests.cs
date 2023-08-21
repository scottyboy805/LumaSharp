using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Statements
{
    [TestClass]
    public class SelectStatementTests
    {
        [TestMethod]
        public void SelectEmpty()
        {
            string input = "select(5){}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.SelectStatementContext assign = context.GetChild<LumaSharpParser.SelectStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("select", assign.GetChild(0).GetText());
        }

        [TestMethod]
        public void SelectDefaultEmpty()
        {
            string input = "select(5){default:;}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.SelectStatementContext assign = context.GetChild<LumaSharpParser.SelectStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("select", assign.GetChild(0).GetText());
        }

        [TestMethod]
        public void SelectDefaultMultiple()
        {
            string input = "select(5){default:;default:;}";
            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputStringStatement(input));
        }

        [TestMethod]
        public void SelectDefault()
        {
            string input = "select(5){default: return 20;}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.SelectStatementContext assign = context.GetChild<LumaSharpParser.SelectStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("select", assign.GetChild(0).GetText());
        }

        [TestMethod]
        public void SelectDefaultLast()
        {
            string input = "select(5){match 5:;default: return 20;}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.SelectStatementContext assign = context.GetChild<LumaSharpParser.SelectStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("select", assign.GetChild(0).GetText());
        }

        [TestMethod]
        public void SelectMatchEmpty()
        {
            string input = "select(5){match 5:;}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.SelectStatementContext assign = context.GetChild<LumaSharpParser.SelectStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("select", assign.GetChild(0).GetText());
        }

        [TestMethod]
        public void SelectMatch()
        {
            string input = "select(5){match 5: return 20;}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.SelectStatementContext assign = context.GetChild<LumaSharpParser.SelectStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("select", assign.GetChild(0).GetText());
        }
    }
}
