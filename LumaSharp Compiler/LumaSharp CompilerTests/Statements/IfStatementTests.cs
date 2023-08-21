using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Statements
{
    [TestClass]
    public class IfStatementTests
    {
        [TestMethod]
        public void IfEmptyInline()
        {
            string input = "if(true);";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
        }

        [TestMethod]
        public void IfEmptyBlock()
        {
            string input = "if(true){}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
        }

        [TestMethod]
        public void ElseIfEmptyInline()
        {
            string input = "if(true); elseif(false);";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }

        [TestMethod]
        public void ElseIfEmptyBlock()
        {
            string input = "if(true){} elseif(false){}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }

        [TestMethod]
        public void ElseIfEmptyInlineBlock()
        {
            string input = "if(true); elseif(false){}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }

        [TestMethod]
        public void ElseIfEmptyBlockInline()
        {
            string input = "if(true){} elseif(false);";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }
    }
}
