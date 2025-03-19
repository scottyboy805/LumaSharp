//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace LumaSharp_CompilerTests.ParseTree.Declarations
//{
//    [TestClass]
//    public class NamespaceDeclarationTests
//    {
//        [TestMethod]
//        public void Namespace()
//        {
//            string input = "namespace Test{}";
//            LumaSharpParser.NamespaceDeclarationContext context = TestUtils.ParseNamespaceDeclaration(input);

//            // Check for root namespace
//            Assert.IsNotNull(context);

//            // Check for namespace declaration
//            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.NamespaceDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test", context.namespaceName().IDENTIFIER().GetText());
//        }

//        [TestMethod]
//        public void NamespaceMultiple()
//        {
//            string input = "namespace Test1{}namespace Test2{}";
//            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

//            // Check for root namespace
//            Assert.IsNotNull(context);

//            // Check for namespace declaration
//            LumaSharpParser.NamespaceDeclarationContext name1 = context.rootElement(0).namespaceDeclaration();
//            Assert.IsInstanceOfType(name1, typeof(LumaSharpParser.NamespaceDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test1", name1.namespaceName().GetText());


//            // Check for namespace declaration
//            LumaSharpParser.NamespaceDeclarationContext name2 = context.rootElement(1).namespaceDeclaration();
//            Assert.IsInstanceOfType(name2, typeof(LumaSharpParser.NamespaceDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test2", name2.namespaceName().IDENTIFIER().GetText());
//        }

//        [TestMethod]
//        public void Namespace_WithType()
//        {
//            string input = "namespace Test{type MyType{}}";
//            LumaSharpParser.NamespaceDeclarationContext context = TestUtils.ParseNamespaceDeclaration(input);

//            // Check for root namespace
//            Assert.IsNotNull(context);

//            // Check for namespace name
//            Assert.AreEqual("Test", context.namespaceName().GetText());

//            // Check for Type declaration
//            LumaSharpParser.TypeDeclarationContext type1 = context.rootMemberBlock().rootMember(0).typeDeclaration();
//            Assert.IsInstanceOfType(type1, typeof(LumaSharpParser.TypeDeclarationContext));

//            // Check for type name
//            Assert.AreEqual("MyType", type1.IDENTIFIER().GetText());
//        }

//        [TestMethod]
//        public void Namespace_WithMultipleTypes()
//        {
//            string input = "namespace Test{type MyType1{}type MyType2{}}";
//            LumaSharpParser.NamespaceDeclarationContext context = TestUtils.ParseNamespaceDeclaration(input);

//            // Check for root namespace
//            Assert.IsNotNull(context);

//            // Check for namespace name
//            Assert.AreEqual("Test", context.namespaceName().GetText());



//            // Check for instance of
//            LumaSharpParser.TypeDeclarationContext type1 = context.rootMemberBlock().rootMember(0).typeDeclaration();
//            Assert.IsInstanceOfType(type1, typeof(LumaSharpParser.TypeDeclarationContext));

//            // Check for type name
//            Assert.AreEqual("MyType1", type1.IDENTIFIER().GetText());


//            // Check for instance of
//            LumaSharpParser.TypeDeclarationContext type2 = context.rootMemberBlock().rootMember(1).typeDeclaration();
//            Assert.IsInstanceOfType(type2, typeof(LumaSharpParser.TypeDeclarationContext));

//            // Check for type name
//            Assert.AreEqual("MyType2", type2.IDENTIFIER().GetText());
//        }
//    }
//}