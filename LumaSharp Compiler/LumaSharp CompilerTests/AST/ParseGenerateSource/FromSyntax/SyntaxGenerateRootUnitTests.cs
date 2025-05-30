﻿using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword));

            // Get expression text
            Assert.AreEqual("export type MyType{}", syntax1.GetSourceText());
            Assert.AreEqual("export", syntax1.StartToken.Text);
            Assert.AreEqual("}", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Type("MyType")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword));

            // Get expression text
            Assert.AreEqual("export global type MyType{}", syntax2.GetSourceText());
            Assert.AreEqual("export", syntax2.StartToken.Text);
            Assert.AreEqual("}", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Type("MyType")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")));

            // Get expression text
            Assert.AreEqual("#Tag export global type MyType{}", syntax3.GetSourceText());
            Assert.AreEqual("#", syntax3.StartToken.Text);
            Assert.AreEqual("}", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.Type("MyType")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false))));

            // Get expression text
            Assert.AreEqual("#Tag(5,false)export global type MyType{}", syntax4.GetSourceText());
            Assert.AreEqual("#", syntax4.StartToken.Text);
            Assert.AreEqual("}", syntax4.EndToken.Text);

            SyntaxNode syntax5 = Syntax.Type("MyType")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")), Syntax.Attribute(Syntax.TypeReference("Range")));

            // Get expression text
            Assert.AreEqual("#Tag #Range export global type MyType{}", syntax5.GetSourceText());
            Assert.AreEqual("#", syntax5.StartToken.Text);
            Assert.AreEqual("}", syntax5.EndToken.Text);

            SyntaxNode syntax6 = Syntax.Type("MyType")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false))),
                Syntax.Attribute(Syntax.TypeReference("Range"), Syntax.ArgumentList(Syntax.Literal(true))));

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

            SyntaxNode syntax11 = Syntax.Type("MyType")
                .WithBaseTypes(Syntax.TypeReference("MyBase"));

            // Get expression text
            Assert.AreEqual("type MyType:MyBase{}", syntax11.GetSourceText());
            Assert.AreEqual("type", syntax11.StartToken.Text);
            Assert.AreEqual("}", syntax11.EndToken.Text);

            SyntaxNode syntax12 = Syntax.Type("MyType")
                .WithBaseTypes(Syntax.TypeReference("MyBase"), Syntax.TypeReference("MyContract"));

            // Get expression text
            Assert.AreEqual("type MyType:MyBase,MyContract{}", syntax12.GetSourceText());
            Assert.AreEqual("type", syntax12.StartToken.Text);
            Assert.AreEqual("}", syntax12.EndToken.Text);
        }

        [TestMethod]
        public void GenerateRoot_Contract()
        {
            SyntaxNode syntax0 = Syntax.Contract("MyContract");

            // Get expression text
            Assert.AreEqual("contract MyContract{}", syntax0.GetSourceText());
            Assert.AreEqual("contract", syntax0.StartToken.Text);
            Assert.AreEqual("}", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Contract("MyContract")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword));

            // Get expression text
            Assert.AreEqual("export contract MyContract{}", syntax1.GetSourceText());
            Assert.AreEqual("export", syntax1.StartToken.Text);
            Assert.AreEqual("}", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Contract("MyContract")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword));

            // Get expression text
            Assert.AreEqual("export global contract MyContract{}", syntax2.GetSourceText());
            Assert.AreEqual("export", syntax2.StartToken.Text);
            Assert.AreEqual("}", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Contract("MyContract")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")));

            // Get expression text
            Assert.AreEqual("#Tag export global contract MyContract{}", syntax3.GetSourceText());
            Assert.AreEqual("#", syntax3.StartToken.Text);
            Assert.AreEqual("}", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.Contract("MyContract")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false))));

            // Get expression text
            Assert.AreEqual("#Tag(5,false)export global contract MyContract{}", syntax4.GetSourceText());
            Assert.AreEqual("#", syntax4.StartToken.Text);
            Assert.AreEqual("}", syntax4.EndToken.Text);

            SyntaxNode syntax5 = Syntax.Contract("MyContract")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")), Syntax.Attribute(Syntax.TypeReference("Range")));

            // Get expression text
            Assert.AreEqual("#Tag #Range export global contract MyContract{}", syntax5.GetSourceText());
            Assert.AreEqual("#", syntax5.StartToken.Text);
            Assert.AreEqual("}", syntax5.EndToken.Text);

            SyntaxNode syntax6 = Syntax.Contract("MyContract")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false))),
                Syntax.Attribute(Syntax.TypeReference("Range"), Syntax.ArgumentList(Syntax.Literal(true))));

            // Get expression text
            Assert.AreEqual("#Tag(5,false)#Range(true)export global contract MyContract{}", syntax6.GetSourceText());
            Assert.AreEqual("#", syntax6.StartToken.Text);
            Assert.AreEqual("}", syntax6.EndToken.Text);


            SyntaxNode syntax7 = Syntax.Contract("MyContract")
                .WithGenericParameters(Syntax.GenericParameter("T"));

            // Get expression text
            Assert.AreEqual("contract MyContract<T>{}", syntax7.GetSourceText());
            Assert.AreEqual("contract", syntax7.StartToken.Text);
            Assert.AreEqual("}", syntax7.EndToken.Text);

            SyntaxNode syntax8 = Syntax.Contract("MyContract")
                .WithGenericParameters(Syntax.GenericParameter("T0"), Syntax.GenericParameter("T1"));

            // Get expression text
            Assert.AreEqual("contract MyContract<T0,T1>{}", syntax8.GetSourceText());
            Assert.AreEqual("contract", syntax8.StartToken.Text);
            Assert.AreEqual("}", syntax8.EndToken.Text);

            SyntaxNode syntax9 = Syntax.Contract("MyContract")
                .WithGenericParameters(Syntax.GenericParameter("T0", Syntax.TypeReference("CDispose")), Syntax.GenericParameter("T1"));

            // Get expression text
            Assert.AreEqual("contract MyContract<T0:CDispose,T1>{}", syntax9.GetSourceText());
            Assert.AreEqual("contract", syntax9.StartToken.Text);
            Assert.AreEqual("}", syntax9.EndToken.Text);

            SyntaxNode syntax10 = Syntax.Contract("MyContract")
                .WithGenericParameters(Syntax.GenericParameter("T0", Syntax.TypeReference("CDispose"), Syntax.TypeReference("enum")), Syntax.GenericParameter("T1"));

            // Get expression text
            Assert.AreEqual("contract MyContract<T0:CDispose:enum,T1>{}", syntax10.GetSourceText());
            Assert.AreEqual("contract", syntax10.StartToken.Text);
            Assert.AreEqual("}", syntax10.EndToken.Text);

            SyntaxNode syntax11 = Syntax.Contract("MyContract")
                .WithBaseTypes(Syntax.TypeReference("MyBase"));

            // Get expression text
            Assert.AreEqual("contract MyContract:MyBase{}", syntax11.GetSourceText());
            Assert.AreEqual("contract", syntax11.StartToken.Text);
            Assert.AreEqual("}", syntax11.EndToken.Text);

            SyntaxNode syntax12 = Syntax.Contract("MyContract")
                .WithBaseTypes(Syntax.TypeReference("MyBase"), Syntax.TypeReference("MyOtherContract"));

            // Get expression text
            Assert.AreEqual("contract MyContract:MyBase,MyOtherContract{}", syntax12.GetSourceText());
            Assert.AreEqual("contract", syntax12.StartToken.Text);
            Assert.AreEqual("}", syntax12.EndToken.Text);
        }

        [TestMethod]
        public void GenerateRoot_Enum()
        {
            SyntaxNode syntax0 = Syntax.Enum("MyEnum");

            Assert.AreEqual("enum MyEnum{}", syntax0.GetSourceText());
            Assert.AreEqual("enum", syntax0.StartToken.Text);
            Assert.AreEqual("}", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Enum("MyEnum", Syntax.TypeReference(PrimitiveType.I8));

            Assert.AreEqual("enum MyEnum:i8{}", syntax1.GetSourceText());
            Assert.AreEqual("enum", syntax1.StartToken.Text);
            Assert.AreEqual("}", syntax1.EndToken.Text);


            SyntaxNode syntax2 = Syntax.Enum("MyEnum")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword));

            // Get expression text
            Assert.AreEqual("export enum MyEnum{}", syntax2.GetSourceText());
            Assert.AreEqual("export", syntax2.StartToken.Text);
            Assert.AreEqual("}", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Enum("MyEnum")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword));

            // Get expression text
            Assert.AreEqual("export global enum MyEnum{}", syntax3.GetSourceText());
            Assert.AreEqual("export", syntax3.StartToken.Text);
            Assert.AreEqual("}", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.Enum("MyEnum")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")));

            // Get expression text
            Assert.AreEqual("#Tag export global enum MyEnum{}", syntax4.GetSourceText());
            Assert.AreEqual("#", syntax4.StartToken.Text);
            Assert.AreEqual("}", syntax4.EndToken.Text);

            SyntaxNode syntax5 = Syntax.Enum("MyEnum")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false))));

            // Get expression text
            Assert.AreEqual("#Tag(5,false)export global enum MyEnum{}", syntax5.GetSourceText());
            Assert.AreEqual("#", syntax5.StartToken.Text);
            Assert.AreEqual("}", syntax5.EndToken.Text);

            SyntaxNode syntax6 = Syntax.Enum("MyEnum")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")), Syntax.Attribute(Syntax.TypeReference("Range")));

            // Get expression text
            Assert.AreEqual("#Tag #Range export global enum MyEnum{}", syntax6.GetSourceText());
            Assert.AreEqual("#", syntax6.StartToken.Text);
            Assert.AreEqual("}", syntax6.EndToken.Text);

            SyntaxNode syntax7 = Syntax.Enum("MyEnum")
                .WithAccessModifiers(Syntax.KeywordOrSymbol(SyntaxTokenKind.ExportKeyword), Syntax.KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false))),
                Syntax.Attribute(Syntax.TypeReference("Range"), Syntax.ArgumentList(Syntax.Literal(true))));

            // Get expression text
            Assert.AreEqual("#Tag(5,false)#Range(true)export global enum MyEnum{}", syntax7.GetSourceText());
            Assert.AreEqual("#", syntax7.StartToken.Text);
            Assert.AreEqual("}", syntax7.EndToken.Text);
        }
    }
}
