using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST.ParseGenerateSource.FromSyntax
{
    [TestClass]
    public sealed class SyntaxGenerateMemberUnitTests
    {
        [TestMethod]
        public void GenerateMember_Field()
        {
            SyntaxNode syntax0 = Syntax.Field("MyField", Syntax.TypeReference(PrimitiveType.I32));

            // Get expression text
            Assert.AreEqual("i32 MyField;", syntax0.GetSourceText());
            Assert.AreEqual("i32", syntax0.StartToken.Text);
            Assert.AreEqual(";", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Field("MyField", Syntax.TypeReference("MyType"), 
                Syntax.Literal(5));

            // Get expression text
            Assert.AreEqual("MyType MyField=5;", syntax1.GetSourceText());
            Assert.AreEqual("MyType", syntax1.StartToken.Text);
            Assert.AreEqual(";", syntax1.EndToken.Text);
        }

        [TestMethod]
        public void GenerateMember_Accessor()
        {
            SyntaxNode syntax0 = Syntax.Accessor("MyAccessor", Syntax.TypeReference(PrimitiveType.I32));

            // Get expression text
            Assert.AreEqual("i32 MyAccessor;", syntax0.GetSourceText());
            Assert.AreEqual("i32", syntax0.StartToken.Text);
            Assert.AreEqual(";", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Accessor("MyAccessor", Syntax.TypeReference("MyType"), Syntax.Literal(true));

            // Get expression text
            Assert.AreEqual("MyType MyAccessor=>true;", syntax1.GetSourceText());
            Assert.AreEqual("MyType", syntax1.StartToken.Text);
            Assert.AreEqual(";", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Accessor("MyAccessor", Syntax.TypeReference("MyType"))
                .WithReadStatement(Syntax.Return(Syntax.Literal(true)));

            // Get expression text
            Assert.AreEqual("MyType MyAccessor=>read:return true;", syntax2.GetSourceText());
            Assert.AreEqual("MyType", syntax2.StartToken.Text);
            Assert.AreEqual(";", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Accessor("MyAccessor", Syntax.TypeReference("MyType"))
                .WithReadStatements(Syntax.Return(Syntax.Literal(true)));

            // Get expression text
            Assert.AreEqual("MyType MyAccessor=>read:{return true;}", syntax3.GetSourceText());
            Assert.AreEqual("MyType", syntax3.StartToken.Text);
            Assert.AreEqual("}", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.Accessor("MyAccessor", Syntax.TypeReference("MyType"))
                .WithWriteStatement(Syntax.Return(Syntax.Literal(true)));

            // Get expression text
            Assert.AreEqual("MyType MyAccessor=>write:return true;", syntax4.GetSourceText());
            Assert.AreEqual("MyType", syntax4.StartToken.Text);
            Assert.AreEqual(";", syntax4.EndToken.Text);

            SyntaxNode syntax5 = Syntax.Accessor("MyAccessor", Syntax.TypeReference("MyType"))
                .WithWriteStatements(Syntax.Return(Syntax.Literal(true)));

            // Get expression text
            Assert.AreEqual("MyType MyAccessor=>write:{return true;}", syntax5.GetSourceText());
            Assert.AreEqual("MyType", syntax5.StartToken.Text);
            Assert.AreEqual("}", syntax5.EndToken.Text);

            SyntaxNode syntax6 = Syntax.Accessor("MyAccessor", Syntax.TypeReference("MyType"))
                .WithReadStatements(Syntax.Return(Syntax.Literal(false)))
                .WithWriteStatements(Syntax.Return(Syntax.Literal(true)));

            // Get expression text
            Assert.AreEqual("MyType MyAccessor=>read:{return false;}=>write:{return true;}", syntax6.GetSourceText());
            Assert.AreEqual("MyType", syntax6.StartToken.Text);
            Assert.AreEqual("}", syntax6.EndToken.Text);
        }
    }
}
