using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.ParseGenerateSource.FromSyntax
{
    [TestClass]
    public sealed class SyntaxGenerateMemberUnitTests
    {
        [TestMethod]
        public void GenerateMember_Field()
        {
            SyntaxNode syntax0 = Syntax.Field("MyField", Syntax.TypeReference(PrimitiveType.I32))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("i32 MyField;", syntax0.GetSourceText());
            Assert.AreEqual("i32", syntax0.StartToken.Text);
            Assert.AreEqual(";", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Field("MyField", Syntax.TypeReference("MyType"), 
                Syntax.VariableAssignment(Syntax.Literal(5)));

            // Get expression text
            Assert.AreEqual("MyTypeMyField=5;", syntax1.GetSourceText());
            Assert.AreEqual("MyType", syntax1.StartToken.Text);
            Assert.AreEqual(";", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Field("MyField", Syntax.TypeReference("MyType"),
                Syntax.VariableAssignment(Syntax.Literal(25)))
                    .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("MyType MyField = 25;", syntax2.GetSourceText());
            Assert.AreEqual("MyType", syntax2.StartToken.Text);
            Assert.AreEqual(";", syntax2.EndToken.Text);
        }

        [TestMethod]
        public void GenerateMember_Accessor()
        {
            SyntaxNode syntax0 = Syntax.Accessor("MyAccessor", Syntax.TypeReference(PrimitiveType.I32));

            // Get expression text
            Assert.AreEqual("i32 MyAccessor;", syntax0.GetSourceText());
            Assert.AreEqual("i32", syntax0.StartToken.Text);
            Assert.AreEqual(";", syntax0.EndToken.Text);

            //SyntaxNode syntax1 = Syntax.Accessor("MyAccessor", Syntax.TypeReference("MyType"), Syntax.Literal(true));

            //// Get expression text
            //Assert.AreEqual("MyType MyAccessor=>true;", syntax1.GetSourceText());
            //Assert.AreEqual("MyType", syntax1.StartToken.Text);
            //Assert.AreEqual(";", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Accessor("MyAccessor", Syntax.TypeReference("MyType"))
                .WithAccessorLambda(Syntax.Return(Syntax.Literal(true)));

            // Get expression text
            Assert.AreEqual("MyType MyAccessor=>read:return true;", syntax2.GetSourceText());
            Assert.AreEqual("MyType", syntax2.StartToken.Text);
            Assert.AreEqual(";", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Accessor("MyAccessor", Syntax.TypeReference("MyType"))
                .WithAccessorLambda(Syntax.Return(Syntax.Literal(true)));

            // Get expression text
            Assert.AreEqual("MyType MyAccessor=>read:{return true;}", syntax3.GetSourceText());
            Assert.AreEqual("MyType", syntax3.StartToken.Text);
            Assert.AreEqual("}", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.Accessor("MyAccessor", Syntax.TypeReference("MyType"))
                .WithAccessorLambda(Syntax.Return(Syntax.Literal(true)));

            // Get expression text
            Assert.AreEqual("MyType MyAccessor=>write:return true;", syntax4.GetSourceText());
            Assert.AreEqual("MyType", syntax4.StartToken.Text);
            Assert.AreEqual(";", syntax4.EndToken.Text);

            SyntaxNode syntax5 = Syntax.Accessor("MyAccessor", Syntax.TypeReference("MyType"))
                .WithAccessorLambda(Syntax.Return(Syntax.Literal(true)));

            // Get expression text
            Assert.AreEqual("MyType MyAccessor=>write:{return true;}", syntax5.GetSourceText());
            Assert.AreEqual("MyType", syntax5.StartToken.Text);
            Assert.AreEqual("}", syntax5.EndToken.Text);

            SyntaxNode syntax6 = Syntax.Accessor("MyAccessor", Syntax.TypeReference("MyType"))
                .WithAccessorBody(
                    Syntax.AccessorRead(Syntax.Return(Syntax.Literal(false))),
                    Syntax.AccessorWrite(Syntax.Return(Syntax.Literal(true))));

            // Get expression text
            Assert.AreEqual("MyType MyAccessor=>read:{return false;}=>write:{return true;}", syntax6.GetSourceText());
            Assert.AreEqual("MyType", syntax6.StartToken.Text);
            Assert.AreEqual("}", syntax6.EndToken.Text);
        }

        [TestMethod]
        public void GenerateMember_Method()
        {
            SyntaxNode syntax0 = Syntax.Method("MyMethod", Syntax.TypeReference(PrimitiveType.I32))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("i32 MyMethod();", syntax0.GetSourceText());
            Assert.AreEqual("i32", syntax0.StartToken.Text);
            Assert.AreEqual(";", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Method("MyMethod", Syntax.TypeReference(PrimitiveType.I32))
                .WithBody()
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("i32 MyMethod()\n{\n}", syntax1.GetSourceText());
            Assert.AreEqual("i32", syntax1.StartToken.Text);
            Assert.AreEqual("}", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Method("MyMethod", Syntax.TypeReference(PrimitiveType.I32))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "MyParam"))
                .WithBody()
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("i32 MyMethod(i32 MyParam)\n{\n}", syntax2.GetSourceText());
            Assert.AreEqual("i32", syntax2.StartToken.Text);
            Assert.AreEqual("}", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Method("MyMethod", Syntax.TypeReference(PrimitiveType.I32))
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "MyParam"), Syntax.Parameter(Syntax.TypeReference("MyType"), "Extra"))
                .WithBody()
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("i32 MyMethod(i32 MyParam, MyType Extra)\n{\n}", syntax3.GetSourceText());
            Assert.AreEqual("i32", syntax3.StartToken.Text);
            Assert.AreEqual("}", syntax3.EndToken.Text);
        }
    }
}
