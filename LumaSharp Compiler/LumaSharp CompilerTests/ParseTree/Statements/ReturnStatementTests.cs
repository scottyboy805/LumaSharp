//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace LumaSharp_CompilerTests.ParseTree.Statements
//{
//    [TestClass]
//    public class ReturnStatementTests
//    {
//        [TestMethod]
//        public void Return()
//        {
//            string input = "return;";
//            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

//            // Check for valid
//            Assert.IsNotNull(context);
//            Assert.AreEqual("return", context.GetChild(0).GetChild(0).GetText());
//        }

//        [TestMethod]
//        public void ReturnNumber()
//        {
//            string input = "return 5;";
//            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

//            // Check for valid
//            Assert.IsNotNull(context);
//            Assert.AreEqual("return", context.GetRuleContext<LumaSharpParser.ReturnStatementContext>(0).GetChild(0).GetText());

//            // Get expression
//            Assert.AreEqual("5", context.GetRuleContext<LumaSharpParser.ReturnStatementContext>(0).GetChild(1).GetText());
//        }

//        [TestMethod]
//        public void ReturnIdentifier()
//        {
//            string input = "return MyValue;";
//            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

//            // Check for valid
//            Assert.IsNotNull(context);
//            Assert.AreEqual("return", context.GetRuleContext<LumaSharpParser.ReturnStatementContext>(0).GetChild(0).GetText());

//            // Get expression
//            Assert.AreEqual("MyValue", context.GetRuleContext<LumaSharpParser.ReturnStatementContext>(0).GetChild(1).GetText());
//        }

//        [TestMethod]
//        public void ReturnLiteral()
//        {
//            string input = @"return ""Hello"";";
//            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

//            // Check for valid
//            Assert.IsNotNull(context);
//            Assert.AreEqual("return", context.GetRuleContext<LumaSharpParser.ReturnStatementContext>(0).GetChild(0).GetText());

//            // Get expression
//            Assert.AreEqual(@"""Hello""", context.GetRuleContext<LumaSharpParser.ReturnStatementContext>(0).GetChild(1).GetText());
//        }
//    }
//}
