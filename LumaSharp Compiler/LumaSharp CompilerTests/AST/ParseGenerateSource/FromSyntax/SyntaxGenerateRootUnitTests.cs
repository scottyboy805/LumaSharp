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
        }
    }
}
