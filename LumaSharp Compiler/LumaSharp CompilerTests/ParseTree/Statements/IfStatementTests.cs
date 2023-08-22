using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Statements
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
        public void IfInline()
        {
            string input = "if(true) return 20;";
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
        public void ElseIfInline()
        {
            string input = "if(true); elseif(false) return 50;";
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

        [TestMethod]
        public void ElseEmptyInline()
        {
            string input = "if(true); else;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
        }

        [TestMethod]
        public void ElseInline()
        {
            string input = "if(true); else return 50;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
        }

        [TestMethod]
        public void ElseEmptyBlock()
        {
            string input = "if(true){} else{}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
        }

        [TestMethod]
        public void ElseEmptyInlineBlock()
        {
            string input = "if(true); else{}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
        }

        [TestMethod]
        public void ElseEmptyBlockInline()
        {
            string input = "if(true){} else;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
        }

        [TestMethod]
        public void ElseIfElseEmptyInline()
        {
            string input = "if(true); elseif(false); else;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }

        [TestMethod]
        public void ElseIfElseEmptyBlock()
        {
            string input = "if(true){} elseif(false){} else{}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }

        [TestMethod]
        public void ElseIfElseEmptyInlineInlineBlock()
        {
            string input = "if(true); elseif(false); else{}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }

        [TestMethod]
        public void ElseIfElseEmptyInlineBlockInline()
        {
            string input = "if(true); elseif(false){} else;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }

        [TestMethod]
        public void ElseIfElseEmptyBlockInlineInline()
        {
            string input = "if(true){} elseif(false); else;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }

        [TestMethod]
        public void ElseIfElseEmptyBlockBlockInline()
        {
            string input = "if(true){} elseif(false){} else;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }

        [TestMethod]
        public void ElseIfElseEmptyInlineBlockBlock()
        {
            string input = "if(true); elseif(false){} else{}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }

        [TestMethod]
        public void ElseIfElseEmptyBlockInlineBlock()
        {
            string input = "if(true){} elseif(false); else{}";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }

        [TestMethod]
        public void ElseIfElseMultipleEmptyInline()
        {
            string input = "if(true); elseif(false); elseif(false); else;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            LumaSharpParser.IfStatementContext ifContext = context.GetChild<LumaSharpParser.IfStatementContext>(0);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.IsNotNull(ifContext);
            Assert.AreEqual("true", ifContext.expression().GetText());
            Assert.AreEqual("false", ifContext.elseifStatement()[0].expression().GetText());
        }

        [TestMethod]
        public void ElseIfElseMultipleEmptyBlock()
        {
            string input = "if(true){} elseif(false){} elseif(false){} else{}";
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
