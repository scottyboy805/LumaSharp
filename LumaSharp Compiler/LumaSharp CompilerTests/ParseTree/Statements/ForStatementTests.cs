//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace LumaSharp_CompilerTests.ParseTree.Statements
//{
//    [TestClass]
//    public class ForStatementTests
//    {
//        [TestMethod]
//        public void ForInline()
//        {
//            string input = "for(int a = 0; a < 10; a++);";
//            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
//            LumaSharpParser.ForStatementContext assign = context.GetChild<LumaSharpParser.ForStatementContext>(0);

//            // Check for valid
//            Assert.IsNotNull(context);
//            Assert.AreEqual("for", assign.GetChild(0).GetText());
//            Assert.AreEqual("int", assign.GetChild(2).GetText());
//            Assert.AreEqual("a", assign.GetChild(3).GetText());
//            Assert.AreEqual("in", assign.GetChild(4).GetText());
//            Assert.AreEqual("collection", assign.GetChild(5).GetText());
//        }
//    }
//}
