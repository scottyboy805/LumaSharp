using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Statements
{
    [TestClass]
    public class PostfixStatementTests
    {
        [TestMethod]
        public void PostFix_Increment()
        {
            string input = "MyIdentifier++;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyIdentifier", context.GetChild<LumaSharpParser.PostfixStatementContext>(0).GetChild(0).GetText());
        }

        [TestMethod]
        public void PostFix_Decrement()
        {
            string input = "MyIdentifier--;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyIdentifier", context.GetChild<LumaSharpParser.PostfixStatementContext>(0).GetChild(0).GetText());
        }

        //[TestMethod]
        //public void PostFix_Type()
        //{
        //    string input = "MyType.MyIdentifier++;";
        //    LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

        //    // Check for valid
        //    Assert.IsNotNull(context);
        //    Assert.AreEqual("MyType.MyIdentifier", context.GetChild<LumaSharpParser.PostfixStatementContext>(0).GetChild(0).GetText());
        //}
    }
}
