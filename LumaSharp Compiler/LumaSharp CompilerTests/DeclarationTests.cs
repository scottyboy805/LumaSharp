using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp_CompilerTests;

namespace LumaSharp_Compiler.Tests
{
    [TestClass]
    public class DeclarationTests
    {
        [TestMethod]
        public void CompilationUnit_Empty()
        {
            string input = "";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for no nodes
            Assert.AreEqual(0, context.ChildCount);
        }

        [TestMethod]
        public void Namespace_Empty()
        {
            string input = "namespace Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root namespace
            LumaSharpParser.NamespaceDeclarationContext rootNamespace;
            Assert.IsNotNull(rootNamespace = context
                .GetChild<LumaSharpParser.CompilationUnitContext>(0)
                .GetChild<LumaSharpParser.NamespaceDeclarationContext>(0));

            // Check for namespace declaration
            Assert.IsInstanceOfType(rootNamespace, typeof(LumaSharpParser.NamespaceDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", rootNamespace.ID().GetText());
        }

        [TestMethod]
        public void Namespace_Multiple()
        {
            string input = "namespace Test1{}namespace Test2{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root namespace
            LumaSharpParser.NamespaceDeclarationContext namespace1;
            Assert.IsNotNull(namespace1 = context
                .GetChild<LumaSharpParser.CompilationUnitContext>(0)
                .GetChild<LumaSharpParser.NamespaceDeclarationContext>(0));

            // Check for namespace declaration
            Assert.IsInstanceOfType(namespace1, typeof(LumaSharpParser.NamespaceDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test1", namespace1.ID().GetText());

            // Check for root namespace
            LumaSharpParser.NamespaceDeclarationContext namespace2;
            Assert.IsNotNull(namespace2 = context
                .GetChild<LumaSharpParser.CompilationUnitContext>(0)
                .GetChild<LumaSharpParser.NamespaceDeclarationContext>(1));

            // Check for namespace declaration
            Assert.IsInstanceOfType(namespace2, typeof(LumaSharpParser.NamespaceDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test2", namespace2.ID().GetText());
        }

        [TestMethod]
        public void Namespace_WithType()
        {
            string input = "namespace Test{type MyType{}}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root namespace
            LumaSharpParser.NamespaceDeclarationContext rootNamespace;            
            Assert.IsNotNull(rootNamespace = context
                .GetChild<LumaSharpParser.CompilationUnitContext>(0)
                .GetChild<LumaSharpParser.NamespaceDeclarationContext>(0));

            // Check for namespace name
            Assert.AreEqual("Test", rootNamespace.ID().GetText());

            // Check for type declaration
            LumaSharpParser.TypeDeclarationContext mainType;
            Assert.IsNotNull(mainType = rootNamespace
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for Type declaration
            Assert.IsInstanceOfType(mainType, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("MyType", mainType.ID().GetText());
        }

        [TestMethod]
        public void Namespace_WithMultipleTypes()
        {
            string input = "namespace Test{type MyType1{}type MyType2{}}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root namespace
            LumaSharpParser.NamespaceDeclarationContext rootNamespace;
            Assert.IsNotNull(rootNamespace = context
                .GetChild<LumaSharpParser.CompilationUnitContext>(0)
                .GetChild<LumaSharpParser.NamespaceDeclarationContext>(0));

            // Check for namespace name
            Assert.AreEqual("Test", rootNamespace.ID().GetText());


            // Check for type 1 declaration
            LumaSharpParser.TypeDeclarationContext type1;
            Assert.IsNotNull(type1 = rootNamespace
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for instance of
            Assert.IsInstanceOfType(type1, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("MyType1", type1.ID().GetText());


            // Check for type 2 declaration
            LumaSharpParser.TypeDeclarationContext type2;
            Assert.IsNotNull(type2 = rootNamespace
                .GetChild<LumaSharpParser.TypeDeclarationContext>(1));

            // Check for instance of
            Assert.IsInstanceOfType(type2, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("MyType2", type2.ID().GetText());
        }

        [TestMethod]
        public void RootType()
        {
            string input = "type Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext rootType;
            Assert.IsNotNull(rootType = context
                .GetChild<LumaSharpParser.CompilationUnitContext>(0)
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(rootType, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", rootType.ID().GetText());
        }

        [TestMethod]
        public void RootTypes_Multiple()
        {
            string input = "type Test1{}type Test2{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root namespace
            LumaSharpParser.TypeDeclarationContext type1;
            Assert.IsNotNull(type1 = context
                .GetChild<LumaSharpParser.CompilationUnitContext>(0)
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for namespace declaration
            Assert.IsInstanceOfType(type1, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test1", type1.ID().GetText());

            // Check for root namespace
            LumaSharpParser.TypeDeclarationContext type2;
            Assert.IsNotNull(type2 = context
                .GetChild<LumaSharpParser.CompilationUnitContext>(0)
                .GetChild<LumaSharpParser.TypeDeclarationContext>(1));

            // Check for namespace declaration
            Assert.IsInstanceOfType(type2, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test2", type2.ID().GetText());
        }

        [TestMethod]
        public void NamespaceRootType_Mixed()
        {
            string input = "namespace Test1{}type Test2{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root namespace
            LumaSharpParser.NamespaceDeclarationContext namespace1;
            Assert.IsNotNull(namespace1 = context
                .GetChild<LumaSharpParser.CompilationUnitContext>(0)
                .GetChild<LumaSharpParser.NamespaceDeclarationContext>(0));

            // Check for namespace declaration
            Assert.IsInstanceOfType(namespace1, typeof(LumaSharpParser.NamespaceDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test1", namespace1.ID().GetText());

            // Check for root namespace
            LumaSharpParser.TypeDeclarationContext type2;
            Assert.IsNotNull(type2 = context
                .GetChild<LumaSharpParser.CompilationUnitContext>(0)
                .GetChild<LumaSharpParser.TypeDeclarationContext>(1));

            // Check for namespace declaration
            Assert.IsInstanceOfType(type2, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test2", type2.ID().GetText());
        }
    }
}