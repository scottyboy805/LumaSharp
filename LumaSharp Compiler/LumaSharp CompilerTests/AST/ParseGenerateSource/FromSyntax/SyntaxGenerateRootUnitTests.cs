using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.ParseGenerateSource.FromSyntax
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

            //SyntaxNode syntax2 = Syntax.ImportAlias("MyAlias", Syntax.TypeReference("MyType"), "MyNamespace", "MySubNamespace", "MyFinalNamespace");

            //// Get expression text
            //Assert.AreEqual("import MyAlias as MyNamespace:MySubNamespace:MyFinalNamespace.MyType;", syntax2.GetSourceText());
            //Assert.AreEqual("import", syntax2.StartToken.Text);
            //Assert.AreEqual(";", syntax2.EndToken.Text);
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
            SyntaxNode syntax0 = Syntax.Type("MyType")
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("type MyType\n{\n}", syntax0.GetSourceText());
            Assert.AreEqual("type", syntax0.StartToken.Text);
            Assert.AreEqual("}", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Type("MyType")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("export type MyType\n{\n}", syntax1.GetSourceText());
            Assert.AreEqual("export", syntax1.StartToken.Text);
            Assert.AreEqual("}", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Type("MyType")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("export global type MyType\n{\n}", syntax2.GetSourceText());
            Assert.AreEqual("export", syntax2.StartToken.Text);
            Assert.AreEqual("}", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Type("MyType")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#Tag export global type MyType\n{\n}", syntax3.GetSourceText());
            Assert.AreEqual("#", syntax3.StartToken.Text);
            Assert.AreEqual("}", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.Type("MyType")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false))))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#Tag(5, false) export global type MyType\n{\n}", syntax4.GetSourceText());
            Assert.AreEqual("#", syntax4.StartToken.Text);
            Assert.AreEqual("}", syntax4.EndToken.Text);

            SyntaxNode syntax5 = Syntax.Type("MyType")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")), Syntax.Attribute(Syntax.TypeReference("Range")))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#Tag #Range export global type MyType\n{\n}", syntax5.GetSourceText());
            Assert.AreEqual("#", syntax5.StartToken.Text);
            Assert.AreEqual("}", syntax5.EndToken.Text);

            SyntaxNode syntax6 = Syntax.Type("MyType")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false))),
                Syntax.Attribute(Syntax.TypeReference("Range"), Syntax.ArgumentList(Syntax.Literal(true))))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#Tag(5, false) #Range(true) export global type MyType\n{\n}", syntax6.GetSourceText());
            Assert.AreEqual("#", syntax6.StartToken.Text);
            Assert.AreEqual("}", syntax6.EndToken.Text);


            SyntaxNode syntax7 = Syntax.Type("MyType")
                .WithGenericParameters(Syntax.GenericParameter("T"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("type MyType<T>\n{\n}", syntax7.GetSourceText());
            Assert.AreEqual("type", syntax7.StartToken.Text);
            Assert.AreEqual("}", syntax7.EndToken.Text);

            SyntaxNode syntax8 = Syntax.Type("MyType")
                .WithGenericParameters(Syntax.GenericParameter("T0"), Syntax.GenericParameter("T1"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("type MyType<T0, T1>\n{\n}", syntax8.GetSourceText());
            Assert.AreEqual("type", syntax8.StartToken.Text);
            Assert.AreEqual("}", syntax8.EndToken.Text);

            SyntaxNode syntax9 = Syntax.Type("MyType")
                .WithGenericParameters(Syntax.GenericParameter("T0", Syntax.TypeReference("CDispose")), Syntax.GenericParameter("T1"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("type MyType<T0 : CDispose, T1>\n{\n}", syntax9.GetSourceText());
            Assert.AreEqual("type", syntax9.StartToken.Text);
            Assert.AreEqual("}", syntax9.EndToken.Text);

            SyntaxNode syntax10 = Syntax.Type("MyType")
                .WithGenericParameters(Syntax.GenericParameter("T0", Syntax.TypeReference("CDispose"), Syntax.TypeReference("enum")), Syntax.GenericParameter("T1"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("type MyType<T0 : CDispose & enum, T1>\n{\n}", syntax10.GetSourceText());
            Assert.AreEqual("type", syntax10.StartToken.Text);
            Assert.AreEqual("}", syntax10.EndToken.Text);

            SyntaxNode syntax11 = Syntax.Type("MyType")
                .WithBaseTypes(Syntax.TypeReference("MyBase"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("type MyType : MyBase\n{\n}", syntax11.GetSourceText());
            Assert.AreEqual("type", syntax11.StartToken.Text);
            Assert.AreEqual("}", syntax11.EndToken.Text);

            SyntaxNode syntax12 = Syntax.Type("MyType")
                .WithBaseTypes(Syntax.TypeReference("MyBase"), Syntax.TypeReference("MyContract"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("type MyType : MyBase, MyContract\n{\n}", syntax12.GetSourceText());
            Assert.AreEqual("type", syntax12.StartToken.Text);
            Assert.AreEqual("}", syntax12.EndToken.Text);

            SyntaxNode syntax13 = Syntax.Type("MyType")
                .WithGenericParameters(Syntax.GenericParameter("T0", Syntax.TypeReference("CDispose"), Syntax.TypeReference("enum")), Syntax.GenericParameter("T1"))
                .WithBaseTypes(Syntax.TypeReference("MyBase"), Syntax.TypeReference("MyContract"), Syntax.TypeReference("OtherContract"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("type MyType<T0 : CDispose & enum, T1> : MyBase, MyContract, OtherContract\n{\n}", syntax13.GetSourceText());
            Assert.AreEqual("type", syntax13.StartToken.Text);
            Assert.AreEqual("}", syntax13.EndToken.Text);

            SyntaxNode syntax14 = Syntax.Type("MyType")
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("copy")), Syntax.Attribute(Syntax.TypeReference("serialize")))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithGenericParameters(Syntax.GenericParameter("T0", Syntax.TypeReference("CDispose"), Syntax.TypeReference("enum")), Syntax.GenericParameter("T1"))
                .WithBaseTypes(Syntax.TypeReference("MyBase"), Syntax.TypeReference("MyContract"), Syntax.TypeReference("OtherContract"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#copy #serialize export global type MyType<T0 : CDispose & enum, T1> : MyBase, MyContract, OtherContract\n{\n}", syntax14.GetSourceText());
            Assert.AreEqual("#", syntax14.StartToken.Text);
            Assert.AreEqual("}", syntax14.EndToken.Text);
        }

        [TestMethod]
        public void GenerateRoot_Contract()
        {
            SyntaxNode syntax0 = Syntax.Contract("MyContract")
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("contract MyContract\n{\n}", syntax0.GetSourceText());
            Assert.AreEqual("contract", syntax0.StartToken.Text);
            Assert.AreEqual("}", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Contract("MyContract")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("export contract MyContract\n{\n}", syntax1.GetSourceText());
            Assert.AreEqual("export", syntax1.StartToken.Text);
            Assert.AreEqual("}", syntax1.EndToken.Text);

            SyntaxNode syntax2 = Syntax.Contract("MyContract")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("export global contract MyContract\n{\n}", syntax2.GetSourceText());
            Assert.AreEqual("export", syntax2.StartToken.Text);
            Assert.AreEqual("}", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Contract("MyContract")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#Tag export global contract MyContract\n{\n}", syntax3.GetSourceText());
            Assert.AreEqual("#", syntax3.StartToken.Text);
            Assert.AreEqual("}", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.Contract("MyContract")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false))))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#Tag(5, false) export global contract MyContract\n{\n}", syntax4.GetSourceText());
            Assert.AreEqual("#", syntax4.StartToken.Text);
            Assert.AreEqual("}", syntax4.EndToken.Text);

            SyntaxNode syntax5 = Syntax.Contract("MyContract")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")), Syntax.Attribute(Syntax.TypeReference("Range")))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#Tag #Range export global contract MyContract\n{\n}", syntax5.GetSourceText());
            Assert.AreEqual("#", syntax5.StartToken.Text);
            Assert.AreEqual("}", syntax5.EndToken.Text);

            SyntaxNode syntax6 = Syntax.Contract("MyContract")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false))),
                Syntax.Attribute(Syntax.TypeReference("Range"), Syntax.ArgumentList(Syntax.Literal(true))))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#Tag(5, false) #Range(true) export global contract MyContract\n{\n}", syntax6.GetSourceText());
            Assert.AreEqual("#", syntax6.StartToken.Text);
            Assert.AreEqual("}", syntax6.EndToken.Text);


            SyntaxNode syntax7 = Syntax.Contract("MyContract")
                .WithGenericParameters(Syntax.GenericParameter("T"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("contract MyContract<T>\n{\n}", syntax7.GetSourceText());
            Assert.AreEqual("contract", syntax7.StartToken.Text);
            Assert.AreEqual("}", syntax7.EndToken.Text);

            SyntaxNode syntax8 = Syntax.Contract("MyContract")
                .WithGenericParameters(Syntax.GenericParameter("T0"), Syntax.GenericParameter("T1"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("contract MyContract<T0, T1>\n{\n}", syntax8.GetSourceText());
            Assert.AreEqual("contract", syntax8.StartToken.Text);
            Assert.AreEqual("}", syntax8.EndToken.Text);

            SyntaxNode syntax9 = Syntax.Contract("MyContract")
                .WithGenericParameters(Syntax.GenericParameter("T0", Syntax.TypeReference("CDispose")), Syntax.GenericParameter("T1"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("contract MyContract<T0 : CDispose, T1>\n{\n}", syntax9.GetSourceText());
            Assert.AreEqual("contract", syntax9.StartToken.Text);
            Assert.AreEqual("}", syntax9.EndToken.Text);

            SyntaxNode syntax10 = Syntax.Contract("MyContract")
                .WithGenericParameters(Syntax.GenericParameter("T0", Syntax.TypeReference("CDispose"), Syntax.TypeReference("enum")), Syntax.GenericParameter("T1"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("contract MyContract<T0 : CDispose & enum, T1>\n{\n}", syntax10.GetSourceText());
            Assert.AreEqual("contract", syntax10.StartToken.Text);
            Assert.AreEqual("}", syntax10.EndToken.Text);

            SyntaxNode syntax11 = Syntax.Contract("MyContract")
                .WithBaseTypes(Syntax.TypeReference("MyBase"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("contract MyContract : MyBase\n{\n}", syntax11.GetSourceText());
            Assert.AreEqual("contract", syntax11.StartToken.Text);
            Assert.AreEqual("}", syntax11.EndToken.Text);

            SyntaxNode syntax12 = Syntax.Contract("MyContract")
                .WithBaseTypes(Syntax.TypeReference("MyBase"), Syntax.TypeReference("MyOtherContract"))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("contract MyContract : MyBase, MyOtherContract\n{\n}", syntax12.GetSourceText());
            Assert.AreEqual("contract", syntax12.StartToken.Text);
            Assert.AreEqual("}", syntax12.EndToken.Text);
        }

        [TestMethod]
        public void GenerateRoot_Enum()
        {
            SyntaxNode syntax0 = Syntax.Enum("MyEnum")
                .NormalizeWhitespace();

            Assert.AreEqual("enum MyEnum\n{\n}", syntax0.GetSourceText());
            Assert.AreEqual("enum", syntax0.StartToken.Text);
            Assert.AreEqual("}", syntax0.EndToken.Text);

            SyntaxNode syntax1 = Syntax.Enum("MyEnum", Syntax.TypeReference(PrimitiveType.I8))
                .NormalizeWhitespace();

            Assert.AreEqual("enum MyEnum : i8\n{\n}", syntax1.GetSourceText());
            Assert.AreEqual("enum", syntax1.StartToken.Text);
            Assert.AreEqual("}", syntax1.EndToken.Text);


            SyntaxNode syntax2 = Syntax.Enum("MyEnum")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("export enum MyEnum\n{\n}", syntax2.GetSourceText());
            Assert.AreEqual("export", syntax2.StartToken.Text);
            Assert.AreEqual("}", syntax2.EndToken.Text);

            SyntaxNode syntax3 = Syntax.Enum("MyEnum")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("export global enum MyEnum\n{\n}", syntax3.GetSourceText());
            Assert.AreEqual("export", syntax3.StartToken.Text);
            Assert.AreEqual("}", syntax3.EndToken.Text);

            SyntaxNode syntax4 = Syntax.Enum("MyEnum")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#Tag export global enum MyEnum\n{\n}", syntax4.GetSourceText());
            Assert.AreEqual("#", syntax4.StartToken.Text);
            Assert.AreEqual("}", syntax4.EndToken.Text);

            SyntaxNode syntax5 = Syntax.Enum("MyEnum")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false))))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#Tag(5, false) export global enum MyEnum\n{\n}", syntax5.GetSourceText());
            Assert.AreEqual("#", syntax5.StartToken.Text);
            Assert.AreEqual("}", syntax5.EndToken.Text);

            SyntaxNode syntax6 = Syntax.Enum("MyEnum")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag")), Syntax.Attribute(Syntax.TypeReference("Range")))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#Tag #Range export global enum MyEnum\n{\n}", syntax6.GetSourceText());
            Assert.AreEqual("#", syntax6.StartToken.Text);
            Assert.AreEqual("}", syntax6.EndToken.Text);

            SyntaxNode syntax7 = Syntax.Enum("MyEnum")
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword), Syntax.Token(SyntaxTokenKind.GlobalKeyword))
                .WithAttributes(Syntax.Attribute(Syntax.TypeReference("Tag"), Syntax.ArgumentList(Syntax.Literal(5), Syntax.Literal(false))),
                Syntax.Attribute(Syntax.TypeReference("Range"), Syntax.ArgumentList(Syntax.Literal(true))))
                .NormalizeWhitespace();

            // Get expression text
            Assert.AreEqual("#Tag(5, false) #Range(true) export global enum MyEnum\n{\n}", syntax7.GetSourceText());
            Assert.AreEqual("#", syntax7.StartToken.Text);
            Assert.AreEqual("}", syntax7.EndToken.Text);
        }
    }
}
