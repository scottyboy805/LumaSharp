using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.ParseGenerateSource.FromSyntax
{
    [TestClass]
    public sealed class SyntaxGenerateStatementUnitTests
    {
        [TestMethod]
        public void GenerateStatement_Assign()
        {
            SyntaxNode syntax = Syntax.Assign(
                Syntax.VariableReference("myVariable"),
                    Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.Literal(5)));

            // Get expression text
            Assert.AreEqual("myVariable=5;", syntax.GetSourceText());
            Assert.AreEqual("myVariable", syntax.StartToken.Text);
            Assert.AreEqual(";", syntax.EndToken.Text);
        }

        [TestMethod]
        public void GenerateStatement_Break()
        {
            SyntaxNode syntax = Syntax.Break();

            // Get expression text
            Assert.AreEqual("break;", syntax.GetSourceText());
            Assert.AreEqual("break", syntax.StartToken.Text);
            Assert.AreEqual(";", syntax.EndToken.Text);
        }

        [TestMethod]
        public void GenerateStatement_Condition()
        {
            SyntaxNode syntax0 = Syntax.Condition(
                Syntax.Binary(Syntax.VariableReference("myVariable"), Syntax.Token(SyntaxTokenKind.EqualitySymbol), Syntax.VariableReference("myOtherVariable")));

            // Get expression text
            Assert.AreEqual("if(myVariable==myOtherVariable);", syntax0.GetSourceText());
            Assert.AreEqual("if", syntax0.StartToken.Text);
            Assert.AreEqual(";", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Condition(
                Syntax.Literal(true))
                .WithInlineStatement(Syntax.Break());

            // Get expression text
            Assert.AreEqual("if(true)break;", syntax1.GetSourceText());
            Assert.AreEqual("if", syntax1.StartToken.Text);
            Assert.AreEqual(";", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Condition(
                Syntax.Literal(true))
                .WithStatementBlock(Syntax.Break());

            // Get expression text
            Assert.AreEqual("if(true){break;}", syntax2.GetSourceText());
            Assert.AreEqual("if", syntax2.StartToken.Text);
            Assert.AreEqual("}", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Condition(
                Syntax.Literal(true))
                .WithInlineStatement(Syntax.Break())
                .WithAlternate(Syntax.Condition()
                    .WithInlineStatement(Syntax.Continue()));

            // Get expression text
            Assert.AreEqual("if(true)break;else continue;", syntax3.GetSourceText());
            Assert.AreEqual("if", syntax3.StartToken.Text);
            Assert.AreEqual(";", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.Condition(
                Syntax.Literal(true))
                .WithInlineStatement(Syntax.Break())
                .WithAlternate(Syntax.Condition(Syntax.Literal(false))
                    .WithInlineStatement(Syntax.Continue()));

            // Get expression text
            Assert.AreEqual("if(true)break;elif(false)continue;", syntax4.GetSourceText());
            Assert.AreEqual("if", syntax4.StartToken.Text);
            Assert.AreEqual(";", syntax4.EndToken.Text);

            SyntaxNode syntax5 = Syntax.Condition(
                Syntax.Literal(true))
                .WithInlineStatement(Syntax.Break())
                .WithAlternate(Syntax.Condition(Syntax.Literal(false))
                    .WithInlineStatement(Syntax.Continue())
                    .WithAlternate(Syntax.Condition()                    
                .WithInlineStatement(Syntax.Assign(Syntax.VariableReference("myVar"),
                    Syntax.VariableAssignment(Syntax.Token(SyntaxTokenKind.AssignSymbol), Syntax.Literal(5))))));

            Assert.AreEqual("if(true)break;elif(false)continue;else myVar=5;", syntax5 .GetSourceText());
            Assert.AreEqual("if", syntax5.StartToken.Text);
            Assert.AreEqual(";", syntax5.EndToken.Text);
        }

        [TestMethod]
        public void GenerateStatement_Continue()
        {
            SyntaxNode syntax = Syntax.Continue();

            // Get expression text
            Assert.AreEqual("continue;", syntax.GetSourceText());
            Assert.AreEqual("continue", syntax.StartToken.Text);
            Assert.AreEqual(";", syntax.EndToken.Text);
        }

        //[TestMethod]
        //public void GenerateStatement_Foreach()
        //{
        //    SyntaxNode syntax0 = Syntax.Foreach(
        //        Syntax.TypeReference(PrimitiveType.I32), "number", Syntax.VariableReference("myNumbers"));

        //    // Get expression text
        //    Assert.AreEqual("foreach(i32 number in myNumbers);", syntax0.GetSourceText());
        //    Assert.AreEqual("foreach", syntax0.StartToken.Text);
        //    Assert.AreEqual(";", syntax0.EndToken.Text);

        //    SyntaxNode syntax1 = Syntax.Foreach(
        //        Syntax.TypeReference("MyType"), "val", Syntax.FieldReference("myField", Syntax.VariableReference("myLocal")))
        //        .WithInlineStatement(Syntax.Break());

        //    // Get expression text
        //    Assert.AreEqual("foreach(MyType val in myLocal.myField)break;", syntax1.GetSourceText());
        //    Assert.AreEqual("foreach", syntax1.StartToken.Text);
        //    Assert.AreEqual(";", syntax1.EndToken.Text);

        //    SyntaxNode syntax2 = Syntax.Foreach(
        //        Syntax.TypeReference("MyType"), "val", Syntax.VariableReference("myLocal"))
        //        .WithStatements(Syntax.Assign(Syntax.VariableReference("local"), Syntax.Literal(5)),
        //        Syntax.Continue());

        //    Assert.AreEqual("foreach(MyType val in myLocal){local=5;continue;}", syntax2.GetSourceText());
        //    Assert.AreEqual("foreach", syntax2.StartToken.Text);
        //    Assert.AreEqual("}", syntax2.EndToken.Text);
        //}

        [TestMethod]
        public void GenerateStatement_For()
        {
            SyntaxNode syntax0 = Syntax.For(null, null, null);

            // Get expression text
            Assert.AreEqual("for(;;);", syntax0.GetSourceText());
            Assert.AreEqual("for", syntax0.StartToken.Text);
            Assert.AreEqual(";", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.For(Syntax.Variable(Syntax.TypeReference("MyType"), "myVar"), null, null);

            // Get expression text
            Assert.AreEqual("for(MyType myVar;;);", syntax1.GetSourceText());
            Assert.AreEqual("for", syntax1.StartToken.Text);
            Assert.AreEqual(";", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.For(null, Syntax.Literal(true), null);

            // Get expression text
            Assert.AreEqual("for(;true;);", syntax2.GetSourceText());
            Assert.AreEqual("for", syntax2.StartToken.Text);
            Assert.AreEqual(";", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.For(null, null, Syntax.Binary(Syntax.VariableReference("i"), Syntax.Token(SyntaxTokenKind.AddSymbol), Syntax.Literal(2)));

            // Get expression text
            Assert.AreEqual("for(;;i+2);", syntax3.GetSourceText());
            Assert.AreEqual("for", syntax3.StartToken.Text);
            Assert.AreEqual(";", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.For(Syntax.Variable(Syntax.TypeReference("MyType"), "myVar"), 
                Syntax.Literal(true), Syntax.Binary(Syntax.VariableReference("i"), Syntax.Token(SyntaxTokenKind.AddSymbol), Syntax.Literal(2)));

            // Get expression text
            Assert.AreEqual("for(MyType myVar;true;i+2);", syntax4.GetSourceText());
            Assert.AreEqual("for", syntax4.StartToken.Text);
            Assert.AreEqual(";", syntax4.EndToken.Text);


            SyntaxNode syntax5 = Syntax.For(null, null, null)
                .WithInlineStatement(Syntax.Break());

            // Get expression text
            Assert.AreEqual("for(;;)break;", syntax5.GetSourceText());
            Assert.AreEqual("for", syntax5.StartToken.Text);
            Assert.AreEqual(";", syntax5.EndToken.Text);

            SyntaxNode syntax6 = Syntax.For(null, null, null)
                .WithStatementBlock(Syntax.Break());

            // Get expression text
            Assert.AreEqual("for(;;){break;}", syntax6.GetSourceText());
            Assert.AreEqual("for", syntax6.StartToken.Text);
            Assert.AreEqual(";", syntax6.EndToken.Text);
        }

        [TestMethod]
        public void GenerateStatement_Return()
        {
            SyntaxNode syntax0 = Syntax.Return();

            // Get expression text
            Assert.AreEqual("return;", syntax0.GetSourceText());
            Assert.AreEqual("return", syntax0.StartToken.Text);
            Assert.AreEqual(";", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Return(Syntax.Literal(5));

            // Get expression text
            Assert.AreEqual("return 5;", syntax1.GetSourceText());
            Assert.AreEqual("return", syntax1.StartToken.Text);
            Assert.AreEqual(";", syntax1.EndToken.Text);
        }

        [TestMethod]
        public void GenerateStatement_Variable()
        {
            SyntaxNode syntax0 = Syntax.Variable(Syntax.TypeReference(PrimitiveType.I32), "var");

            // Get expression text
            Assert.AreEqual("i32 var;", syntax0.GetSourceText());
            Assert.AreEqual("i32", syntax0.StartToken.Text);
            Assert.AreEqual(";", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.LocalVariable(Syntax.TypeReference("MyType"), new SyntaxToken[] { "var1", "var2" });

            // Get expression text
            Assert.AreEqual("MyType var1,var2;", syntax1.GetSourceText());
            Assert.AreEqual("MyType", syntax1.StartToken.Text);
            Assert.AreEqual(";", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Variable(Syntax.TypeReference("MyType"), "var", Syntax.VariableAssignment(Syntax.Literal(5)));

            // Get expression text
            Assert.AreEqual("MyType var=5;", syntax2.GetSourceText());
            Assert.AreEqual("MyType", syntax2.StartToken.Text);
            Assert.AreEqual(";", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Variable(Syntax.TypeReference("MyType"), new SyntaxToken[] { "var1", "var2" }, Syntax.VariableAssignment(Syntax.Literal(5), Syntax.Literal(10)));

            // Get expression text
            Assert.AreEqual("MyType var1,var2={5,10}", syntax3.GetSourceText());
            Assert.AreEqual("MyType", syntax3.StartToken.Text);
            Assert.AreEqual("}", syntax3.EndToken.Text);
        }
    }
}
