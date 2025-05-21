//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace CompilerTests.ParseTree.Statements
//{
//    [TestClass]
//    public class WhileStatementTests
//    {
//        [TestMethod]
//        public void WhileInlineEmpty()
//        {
//            string input = "while(true);";
//            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
//            LumaSharpParser.WhileStatementContext assign = context.GetChild<LumaSharpParser.WhileStatementContext>(0);

//            // Check for valid
//            Assert.IsNotNull(context);
//            Assert.AreEqual("while", assign.GetChild(0).GetText());
//        }

//        [TestMethod]
//        public void WhileInline()
//        {
//            string input = "while(true) return 50;";
//            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
//            LumaSharpParser.WhileStatementContext assign = context.GetChild<LumaSharpParser.WhileStatementContext>(0);

//            // Check for valid
//            Assert.IsNotNull(context);
//            Assert.AreEqual("while", assign.GetChild(0).GetText());
//        }

//        [TestMethod]
//        public void WhileBlockEmpty()
//        {
//            string input = "while(true){}";
//            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
//            LumaSharpParser.WhileStatementContext assign = context.GetChild<LumaSharpParser.WhileStatementContext>(0);

//            // Check for valid
//            Assert.IsNotNull(context);
//            Assert.AreEqual("while", assign.GetChild(0).GetText());
//        }

//        [TestMethod]
//        public void WhileBlock()
//        {
//            string input = "while(true) {return 50;}";
//            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
//            LumaSharpParser.WhileStatementContext assign = context.GetChild<LumaSharpParser.WhileStatementContext>(0);

//            // Check for valid
//            Assert.IsNotNull(context);
//            Assert.AreEqual("while", assign.GetChild(0).GetText());
//        }
//    }
//}
