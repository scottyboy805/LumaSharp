using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Statements
{
    [TestClass]
    public class TryStatementTests
    {
        [TestMethod]
        public void TryInline()
        {
            string input = "try return 5;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.TryStatementContext tryStatement = context.GetChild<LumaSharpParser.TryStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("try", tryStatement.GetChild(0).GetText());
        }

        [TestMethod]
        public void TryBlock()
        {
            string input = "try{ return 5;}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.TryStatementContext tryStatement = context.GetChild<LumaSharpParser.TryStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("try", tryStatement.GetChild(0).GetText());
        }

        [TestMethod]
        public void CatchInline()
        {
            string input = "try return 5;catch return 5;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.TryStatementContext tryStatement = context.GetChild<LumaSharpParser.TryStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("try", tryStatement.GetChild(0).GetText());

            LumaSharpParser.CatchStatementContext catchStatement = tryStatement.catchStatement();

            Assert.IsNotNull(catchStatement);
            Assert.AreEqual("catch", catchStatement.GetChild(0).GetText());
        }

        [TestMethod]
        public void CatchBlock()
        {
            string input = "try {return 5;}catch {return 5;}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.TryStatementContext tryStatement = context.GetChild<LumaSharpParser.TryStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("try", tryStatement.GetChild(0).GetText());

            LumaSharpParser.CatchStatementContext catchStatement = tryStatement.catchStatement();

            Assert.IsNotNull(catchStatement);
            Assert.AreEqual("catch", catchStatement.GetChild(0).GetText());
        }

        [TestMethod]
        public void CatchExceptionInline()
        {
            string input = "try return 5;catch(Exception) return 5;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.TryStatementContext tryStatement = context.GetChild<LumaSharpParser.TryStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("try", tryStatement.GetChild(0).GetText());

            LumaSharpParser.CatchStatementContext catchStatement = tryStatement.catchStatement();

            Assert.IsNotNull(catchStatement);
            Assert.AreEqual("catch", catchStatement.GetChild(0).GetText());
        }

        [TestMethod]
        public void CatchExceptionBlock()
        {
            string input = "try {return 5;}catch(Exception) {return 5;}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.TryStatementContext tryStatement = context.GetChild<LumaSharpParser.TryStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("try", tryStatement.GetChild(0).GetText());

            LumaSharpParser.CatchStatementContext catchStatement = tryStatement.catchStatement();

            Assert.IsNotNull(catchStatement);
            Assert.AreEqual("catch", catchStatement.GetChild(0).GetText());
        }

        [TestMethod]
        public void FinallyInline()
        {
            string input = "try return 5;finally return 5;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.TryStatementContext tryStatement = context.GetChild<LumaSharpParser.TryStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("try", tryStatement.GetChild(0).GetText());

            LumaSharpParser.FinallyStatementContext finallyStatement = tryStatement.finallyStatement();

            Assert.IsNotNull(finallyStatement);
            Assert.AreEqual("finally", finallyStatement.GetChild(0).GetText());
        }

        [TestMethod]
        public void FinallyBlock()
        {
            string input = "try {return 5;}finally {return 5;}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.TryStatementContext tryStatement = context.GetChild<LumaSharpParser.TryStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("try", tryStatement.GetChild(0).GetText());

            LumaSharpParser.FinallyStatementContext finallyStatement = tryStatement.finallyStatement();

            Assert.IsNotNull(finallyStatement);
            Assert.AreEqual("finally", finallyStatement.GetChild(0).GetText());
        }
    }
}
