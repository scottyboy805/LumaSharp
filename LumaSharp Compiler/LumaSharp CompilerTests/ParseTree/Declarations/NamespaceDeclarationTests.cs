using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Declarations
{
    [TestClass]
    public class NamespaceDeclarationTests
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
        public void Namespace()
        {
            string input = "namespace Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root namespace
            LumaSharpParser.NamespaceDeclarationContext rootNamespace;
            Assert.IsNotNull(rootNamespace = context
                .GetChild<LumaSharpParser.NamespaceDeclarationContext>(0));

            // Check for namespace declaration
            Assert.IsInstanceOfType(rootNamespace, typeof(LumaSharpParser.NamespaceDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", rootNamespace.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void NamespaceMultiple()
        {
            string input = "namespace Test1{}namespace Test2{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root namespace
            LumaSharpParser.NamespaceDeclarationContext namespace1;
            Assert.IsNotNull(namespace1 = context
                .GetChild<LumaSharpParser.NamespaceDeclarationContext>(0));

            // Check for namespace declaration
            Assert.IsInstanceOfType(namespace1, typeof(LumaSharpParser.NamespaceDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test1", namespace1.IDENTIFIER().GetText());

            // Check for root namespace
            LumaSharpParser.NamespaceDeclarationContext namespace2;
            Assert.IsNotNull(namespace2 = context
                .GetChild<LumaSharpParser.NamespaceDeclarationContext>(1));

            // Check for namespace declaration
            Assert.IsInstanceOfType(namespace2, typeof(LumaSharpParser.NamespaceDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test2", namespace2.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void Namespace_WithType()
        {
            string input = "namespace Test{type MyType{}}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root namespace
            LumaSharpParser.NamespaceDeclarationContext rootNamespace;
            Assert.IsNotNull(rootNamespace = context
                .GetChild<LumaSharpParser.NamespaceDeclarationContext>(0));

            // Check for namespace name
            Assert.AreEqual("Test", rootNamespace.IDENTIFIER().GetText());

            // Check for type declaration
            LumaSharpParser.TypeDeclarationContext mainType;
            Assert.IsNotNull(mainType = rootNamespace
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for Type declaration
            Assert.IsInstanceOfType(mainType, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("MyType", mainType.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void Namespace_WithMultipleTypes()
        {
            string input = "namespace Test{type MyType1{}type MyType2{}}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root namespace
            LumaSharpParser.NamespaceDeclarationContext rootNamespace;
            Assert.IsNotNull(rootNamespace = context
                .GetChild<LumaSharpParser.NamespaceDeclarationContext>(0));

            // Check for namespace name
            Assert.AreEqual("Test", rootNamespace.IDENTIFIER().GetText());


            // Check for type 1 declaration
            LumaSharpParser.TypeDeclarationContext type1;
            Assert.IsNotNull(type1 = rootNamespace
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for instance of
            Assert.IsInstanceOfType(type1, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("MyType1", type1.IDENTIFIER().GetText());


            // Check for type 2 declaration
            LumaSharpParser.TypeDeclarationContext type2;
            Assert.IsNotNull(type2 = rootNamespace
                .GetChild<LumaSharpParser.TypeDeclarationContext>(1));

            // Check for instance of
            Assert.IsInstanceOfType(type2, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("MyType2", type2.IDENTIFIER().GetText());
        }
    }
}