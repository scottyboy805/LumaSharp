using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.ParseGenerateSource.FromSyntax
{
    [TestClass]
    public sealed class SyntaxGenerateMisc_UnitTests
    {
        [TestMethod]
        public void GenerateMisc_TypeReference()
        {
            SyntaxNode syntax0 = Syntax.TypeReference(PrimitiveType.I32);

            // Get expression text
            Assert.AreEqual("i32", syntax0.GetSourceText());
            Assert.AreEqual("i32", syntax0.StartToken.Text);
            Assert.AreEqual("i32", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.TypeReference("MyType");

            // Get expression text
            Assert.AreEqual("MyType", syntax1.GetSourceText());
            Assert.AreEqual("MyType", syntax1.StartToken.Text);
            Assert.AreEqual("MyType", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.TypeReference(new string[] { "MyNamespace" }, "MyType");

            // Get expression text
            Assert.AreEqual("MyNamespace:MyType", syntax2.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax2.StartToken.Text);
            Assert.AreEqual("MyType", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.TypeReference(new string[] { "MyNamespace", "MySubNamespace" }, "MyType");

            // Get expression text
            Assert.AreEqual("MyNamespace:MySubNamespace:MyType", syntax3.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax3.StartToken.Text);
            Assert.AreEqual("MyType", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.TypeReference(new string[] { "MyNamespace" }, Syntax.ParentTypeReference("SomeType"), "MyType");

            // Get expression text
            Assert.AreEqual("MyNamespace:SomeType.MyType", syntax4.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax4.StartToken.Text);
            Assert.AreEqual("MyType", syntax4.EndToken.Text);

            SyntaxNode syntax5 = Syntax.TypeReference(new string[] { "MyNamespace" }, Syntax.ParentTypeReference("SomeType"), "MyType", null, 1);

            // Get expression text
            Assert.AreEqual("MyNamespace:SomeType.MyType[]", syntax5.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax5.StartToken.Text);
            Assert.AreEqual("]", syntax5.EndToken.Text);

            SyntaxNode syntax6 = Syntax.TypeReference(new string[] { "MyNamespace" }, Syntax.ParentTypeReference("SomeType"), "MyType", null, 2);

            // Get expression text
            Assert.AreEqual("MyNamespace:SomeType.MyType[,]", syntax6.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax6.StartToken.Text);
            Assert.AreEqual("]", syntax6.EndToken.Text);

            SyntaxNode syntax7 = Syntax.TypeReference(new string[] { "MyNamespace" }, Syntax.ParentTypeReference("SomeType"), "MyType",
                Syntax.GenericArgumentList((Syntax.TypeReference(PrimitiveType.I32))), 2);

            // Get expression text
            Assert.AreEqual("MyNamespace:SomeType.MyType<i32>[,]", syntax7.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax7.StartToken.Text);
            Assert.AreEqual("]", syntax7.EndToken.Text);

            SyntaxNode syntax8 = Syntax.TypeReference(new string[] { "MyNamespace" }, Syntax.ParentTypeReference("SomeType"), "MyType", 
                Syntax.GenericArgumentList(Syntax.TypeReference(PrimitiveType.I32), Syntax.TypeReference(new string[] { "NS" }, "OtherType")), 2);

            // Get expression text
            Assert.AreEqual("MyNamespace:SomeType.MyType<i32,NS:OtherType>[,]", syntax8.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax8.StartToken.Text);
            Assert.AreEqual("]", syntax8.EndToken.Text);
        }

        [TestMethod]
        public void GenerateMisc_ParameterList()
        {
            SyntaxNode syntax0 = Syntax.Method("test")
                .WithParameters().Parameters;

            // Get expression text
            Assert.AreEqual("()", syntax0.GetSourceText());
            Assert.AreEqual("(", syntax0.StartToken.Text);
            Assert.AreEqual(")", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Method("test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "val")).Parameters;

            // Get expression text
            Assert.AreEqual("(i32 val)", syntax1.GetSourceText());
            Assert.AreEqual("(", syntax1.StartToken.Text);
            Assert.AreEqual(")", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Method("test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "val"),
                Syntax.Parameter(Syntax.TypeReference("MyType"), "arg")).Parameters;

            // Get expression text
            Assert.AreEqual("(i32 val,MyType arg)", syntax2.GetSourceText());
            Assert.AreEqual("(", syntax2.StartToken.Text);
            Assert.AreEqual(")", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Method("test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "val"),
                Syntax.Parameter(Syntax.TypeReference("MyType"), "arg")).Parameters;

            // Get expression text
            Assert.AreEqual("(i32 val,MyType arg...)", syntax3.GetSourceText());
            Assert.AreEqual("(", syntax3.StartToken.Text);
            Assert.AreEqual(")", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.Method("test")
                .WithParameters(Syntax.Parameter(Syntax.TypeReference(PrimitiveType.I32), "val"),
                Syntax.Parameter(Syntax.TypeReference("MyType"), "arg")
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("ref")))).Parameters;

            // Get expression text
            Assert.AreEqual("(i32 val,MyType& arg)", syntax4.GetSourceText());
            Assert.AreEqual("(", syntax4.StartToken.Text);
            Assert.AreEqual(")", syntax4.EndToken.Text);
        }

        [TestMethod]
        public void GenerateMisc_GenericParameters()
        {
            SyntaxNode syntax0 = Syntax.Type("test")
                .WithGenericParameters(Syntax.GenericParameter("T")).GenericParameters;

            // Get expression text
            Assert.AreEqual("<T>", syntax0.GetSourceText());
            Assert.AreEqual("<", syntax0.StartToken.Text);
            Assert.AreEqual(">", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Type("test")
                .WithGenericParameters(Syntax.GenericParameter("T", Syntax.TypeReference("enum"))).GenericParameters;

            // Get expression text
            Assert.AreEqual("<T:enum>", syntax1.GetSourceText());
            Assert.AreEqual("<", syntax1.StartToken.Text);
            Assert.AreEqual(">", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Type("test")
                .WithGenericParameters(Syntax.GenericParameter("T"), Syntax.GenericParameter("Param")).GenericParameters;

            // Get expression text
            Assert.AreEqual("<T,Param>", syntax2.GetSourceText());
            Assert.AreEqual("<", syntax2.StartToken.Text);
            Assert.AreEqual(">", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Type("test")
                .WithGenericParameters(Syntax.GenericParameter("T", Syntax.TypeReference("enum")), Syntax.GenericParameter("Param", Syntax.TypeReference("MyType"), Syntax.TypeReference("CDispose"))).GenericParameters;

            // Get expression text
            Assert.AreEqual("<T:enum,Param:MyType:CDispose>", syntax3.GetSourceText());
            Assert.AreEqual("<", syntax3.StartToken.Text);
            Assert.AreEqual(">", syntax3.EndToken.Text);
        }
    }
}
