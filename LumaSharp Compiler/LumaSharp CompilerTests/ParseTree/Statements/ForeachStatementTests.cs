using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Statements
{
    [TestClass]
    public class ForeachStatementTests
    {
        [TestMethod]
        public void ForeachInline()
        {
            string input = "foreach(int a in collection);";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.ForeachStatementContext assign = context.GetChild<LumaSharpParser.ForeachStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("foreach", assign.GetChild(0).GetText());
            Assert.AreEqual("int", assign.GetChild(2).GetText());
            Assert.AreEqual("a", assign.GetChild(3).GetText());
            Assert.AreEqual("in", assign.GetChild(4).GetText());
            Assert.AreEqual("collection", assign.GetChild(5).GetText());
        }

        [TestMethod]
        public void ForeachBlock()
        {
            string input = "foreach(int a in collection){}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.ForeachStatementContext assign = context.GetChild<LumaSharpParser.ForeachStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("foreach", assign.GetChild(0).GetText());
            Assert.AreEqual("int", assign.GetChild(2).GetText());
            Assert.AreEqual("a", assign.GetChild(3).GetText());
            Assert.AreEqual("in", assign.GetChild(4).GetText());
            Assert.AreEqual("collection", assign.GetChild(5).GetText());
        }
    }
}
