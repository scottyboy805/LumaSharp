using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST.ParseGenerateSource.FromSyntax
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

            SyntaxNode syntax2 = Syntax.TypeReference("MyType")
                .WithNamespaceQualifier("MyNamespace");

            // Get expression text
            Assert.AreEqual("MyNamespace:MyType", syntax2.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax2.StartToken.Text);
            Assert.AreEqual("MyType", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.TypeReference("MyType")
                .WithNamespaceQualifier("MyNamespace", "MySubNamespace");

            // Get expression text
            Assert.AreEqual("MyNamespace:MySubNamespace:MyType", syntax3.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax3.StartToken.Text);
            Assert.AreEqual("MyType", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.TypeReference("MyType")
                .WithNamespaceQualifier("MyNamespace")
                .WithParentTypeQualifiers(Syntax.TypeReference("SomeType"));

            // Get expression text
            Assert.AreEqual("MyNamespace:SomeType.MyType", syntax4.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax4.StartToken.Text);
            Assert.AreEqual("MyType", syntax4.EndToken.Text);

            SyntaxNode syntax5 = Syntax.TypeReference("MyType")
                .WithNamespaceQualifier("MyNamespace")
                .WithParentTypeQualifiers(Syntax.TypeReference("SomeType"))
                .WithArrayQualifier(1);

            // Get expression text
            Assert.AreEqual("MyNamespace:SomeType.MyType[]", syntax5.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax5.StartToken.Text);
            Assert.AreEqual("]", syntax5.EndToken.Text);

            SyntaxNode syntax6 = Syntax.TypeReference("MyType")
                .WithNamespaceQualifier("MyNamespace")
                .WithParentTypeQualifiers(Syntax.TypeReference("SomeType"))
                .WithArrayQualifier(2)
                .WithReferenceQualifier();

            // Get expression text
            Assert.AreEqual("MyNamespace:SomeType.MyType[,]&", syntax6.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax6.StartToken.Text);
            Assert.AreEqual("&", syntax6.EndToken.Text);

            SyntaxNode syntax7 = Syntax.TypeReference("MyType")
                .WithNamespaceQualifier("MyNamespace")
                .WithParentTypeQualifiers(Syntax.TypeReference("SomeType"))
                .WithGenericArguments(Syntax.TypeReference(PrimitiveType.I32))
                .WithArrayQualifier(2)
                .WithReferenceQualifier();

            // Get expression text
            Assert.AreEqual("MyNamespace:SomeType.MyType<i32>[,]&", syntax7.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax7.StartToken.Text);
            Assert.AreEqual("&", syntax7.EndToken.Text);

            SyntaxNode syntax8 = Syntax.TypeReference("MyType")
                .WithNamespaceQualifier("MyNamespace")
                .WithParentTypeQualifiers(Syntax.TypeReference("SomeType"))
                .WithGenericArguments(Syntax.TypeReference(PrimitiveType.I32), Syntax.TypeReference("OtherType")
                    .WithNamespaceQualifier("NS"))
                .WithArrayQualifier(2)
                .WithReferenceQualifier();

            // Get expression text
            Assert.AreEqual("MyNamespace:SomeType.MyType<i32,NS:OtherType>[,]&", syntax8.GetSourceText());
            Assert.AreEqual("MyNamespace", syntax8.StartToken.Text);
            Assert.AreEqual("&", syntax8.EndToken.Text);
        }
    }
}
