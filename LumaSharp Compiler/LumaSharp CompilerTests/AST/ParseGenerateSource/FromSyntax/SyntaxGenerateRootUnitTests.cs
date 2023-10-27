using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography.X509Certificates;

namespace LumaSharp_CompilerTests.AST.ParseGenerateSource.FromSyntax
{
    [TestClass]
    public sealed class SyntaxGenerateRootUnitTests
    {
        [TestMethod]
        public void GenerateRoot_Import()
        {
            SyntaxNode syntax0 = Syntax.Import("MyNamespace");

            // Get expression text
            Assert.AreEqual("import MyNamespace;", syntax0.GetSourceText());
            Assert.AreEqual("import", syntax0.StartToken.Text);
            Assert.AreEqual(";", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Import("MyNamespace", "MySubNamespace", "MyFinalNamespace");

            // Get expression text
            Assert.AreEqual("import MyNamespace:MySubNamespace:MyFinalNamespace;", syntax1.GetSourceText());
            Assert.AreEqual("import", syntax1.StartToken.Text);
            Assert.AreEqual(";", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.ImportAlias("MyAlias", Syntax.TypeReference("MyType"), "MyNamespace", "MySubNamespace", "MyFinalNamespace");

            // Get expression text
            Assert.AreEqual("import MyAlias as MyNamespace:MySubNamespace:MyFinalNamespace.MyType;", syntax2.GetSourceText());
            Assert.AreEqual("import", syntax2.StartToken.Text);
            Assert.AreEqual(";", syntax2.EndToken.Text);
        }

        [TestMethod]
        public void GenerateRoot_Namespace()
        {
            SyntaxNode syntax0 = Syntax.Namespace("MyNamespace");

            // Get expression text
            Assert.AreEqual("namespace MyNamespace{}", syntax0.GetSourceText());
            Assert.AreEqual("namespace", syntax0.StartToken.Text);
            Assert.AreEqual("}", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Namespace("MyNamespace", "MySubNamespace", "MyFinalNamespace");

            // Get expression text
            Assert.AreEqual("namespace MyNamespace:MySubNamespace:MyFinalNamespace{}", syntax1.GetSourceText());
            Assert.AreEqual("namespace", syntax1.StartToken.Text);
            Assert.AreEqual("}", syntax1.EndToken.Text);
        }

        [TestMethod]
        public void GenerateRoot_Type()
        {
            SyntaxNode syntax0 = Syntax.Type("MyType");

            // Get expression text
            Assert.AreEqual("type MyType{}", syntax0.GetSourceText());
            Assert.AreEqual("type", syntax0.StartToken.Text);
            Assert.AreEqual("}", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Type("MyType")
                .WithAccessModifiers("export");

            // Get expression text
            Assert.AreEqual("export type MyType{}", syntax1.GetSourceText());
            Assert.AreEqual("export", syntax1.StartToken.Text);
            Assert.AreEqual("}", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Type("MyType")
                .WithAccessModifiers("export", "global");

            // Get expression text
            Assert.AreEqual("export global type MyType{}", syntax2.GetSourceText());
            Assert.AreEqual("export", syntax2.StartToken.Text);
            Assert.AreEqual("}", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Type("MyType")
                .WithAccessModifiers("export", "global")
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")));

            // Get expression text
            Assert.AreEqual("#Tag export global type MyType{}", syntax3.GetSourceText());
            Assert.AreEqual("#", syntax3.StartToken.Text);
            Assert.AreEqual("}", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.Type("MyType")
                .WithAccessModifiers("export", "global")
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.Literal(5), Syntax.Literal(false)));

            // Get expression text
            Assert.AreEqual("#Tag(5,false)export global type MyType{}", syntax4.GetSourceText());
            Assert.AreEqual("#", syntax4.StartToken.Text);
            Assert.AreEqual("}", syntax4.EndToken.Text);

            SyntaxNode syntax5 = Syntax.Type("MyType")
                .WithAccessModifiers("export", "global")
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")), Syntax.Attribute(Syntax.TypeReference("Range")));

            // Get expression text
            Assert.AreEqual("#Tag #Range export global type MyType{}", syntax5.GetSourceText());
            Assert.AreEqual("#", syntax5.StartToken.Text);
            Assert.AreEqual("}", syntax5.EndToken.Text);

            SyntaxNode syntax6 = Syntax.Type("MyType")
                .WithAccessModifiers("export", "global")
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.Literal(5), Syntax.Literal(false)),
                Syntax.Attribute(Syntax.TypeReference("Range"), Syntax.Literal(true)));

            // Get expression text
            Assert.AreEqual("#Tag(5,false)#Range(true)export global type MyType{}", syntax6.GetSourceText());
            Assert.AreEqual("#", syntax6.StartToken.Text);
            Assert.AreEqual("}", syntax6.EndToken.Text);


            SyntaxNode syntax7 = Syntax.Type("MyType")
                .WithGenericParameters(Syntax.GenericParameter("T"));

            // Get expression text
            Assert.AreEqual("type MyType<T>{}", syntax7.GetSourceText());
            Assert.AreEqual("type", syntax7.StartToken.Text);
            Assert.AreEqual("}", syntax7.EndToken.Text);

            SyntaxNode syntax8 = Syntax.Type("MyType")
                .WithGenericParameters(Syntax.GenericParameter("T0"), Syntax.GenericParameter("T1"));

            // Get expression text
            Assert.AreEqual("type MyType<T0,T1>{}", syntax8.GetSourceText());
            Assert.AreEqual("type", syntax8.StartToken.Text);
            Assert.AreEqual("}", syntax8.EndToken.Text);

            SyntaxNode syntax9 = Syntax.Type("MyType")
                .WithGenericParameters(Syntax.GenericParameter("T0", Syntax.TypeReference("CDispose")), Syntax.GenericParameter("T1"));

            // Get expression text
            Assert.AreEqual("type MyType<T0:CDispose,T1>{}", syntax9.GetSourceText());
            Assert.AreEqual("type", syntax9.StartToken.Text);
            Assert.AreEqual("}", syntax9.EndToken.Text);

            SyntaxNode syntax10 = Syntax.Type("MyType")
                .WithGenericParameters(Syntax.GenericParameter("T0", Syntax.TypeReference("CDispose"), Syntax.TypeReference("enum")), Syntax.GenericParameter("T1"));

            // Get expression text
            Assert.AreEqual("type MyType<T0:CDispose:enum,T1>{}", syntax10.GetSourceText());
            Assert.AreEqual("type", syntax10.StartToken.Text);
            Assert.AreEqual("}", syntax10.EndToken.Text);
        }
    }
}
