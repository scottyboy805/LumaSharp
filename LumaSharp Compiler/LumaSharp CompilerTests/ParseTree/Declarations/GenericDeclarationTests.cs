//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace LumaSharp_CompilerTests.ParseTree.Declarations
//{
//    [TestClass]
//    public class GenericDeclarationTests
//    {
//        [TestMethod]
//        public void GenericType_1()
//        {
//            string input = "type Test<T>{}";
//            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

//            // Check for root type
//            Assert.IsNotNull(context);

//            // Check for type declaration
//            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

//            LumaSharpParser.GenericParameterListContext generics = context.genericParameterList();

//            Assert.IsNotNull(generics);
//            Assert.AreEqual("<", generics.LGENERIC().GetText());
//            Assert.AreEqual("T", generics.genericParameter().GetText());
//            Assert.AreEqual(">", generics.RGENERIC().GetText());
//        }

//        [TestMethod]
//        public void GenericType_2()
//        {
//            string input = "type Test<T0, T1>{}";
//            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

//            // Check for root type
//            Assert.IsNotNull(context);

//            // Check for type declaration
//            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

//            LumaSharpParser.GenericParameterListContext generics = context.genericParameterList();

//            Assert.IsNotNull(generics);
//            Assert.AreEqual("<", generics.LGENERIC().GetText());
//            Assert.AreEqual("T0", generics.genericParameter().GetText());
//            Assert.AreEqual(",", generics.genericParameterSecondary()[0].COMMA().GetText());
//            Assert.AreEqual("T1", generics.genericParameterSecondary()[0].genericParameter().GetText());
//            Assert.AreEqual(">", generics.RGENERIC().GetText());
//        }

//        [TestMethod]
//        public void GenericType_3()
//        {
//            string input = "type Test<T0, T1, T2>{}";
//            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

//            // Check for root type
//            Assert.IsNotNull(context);

//            // Check for type declaration
//            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

//            LumaSharpParser.GenericParameterListContext generics = context.genericParameterList();

//            Assert.IsNotNull(generics);
//            Assert.AreEqual("<", generics.LGENERIC().GetText());
//            Assert.AreEqual("T0", generics.genericParameter().GetText());
//            Assert.AreEqual(",", generics.genericParameterSecondary()[0].COMMA().GetText());
//            Assert.AreEqual("T1", generics.genericParameterSecondary()[0].genericParameter().GetText());
//            Assert.AreEqual(",", generics.genericParameterSecondary()[1].COMMA().GetText());
//            Assert.AreEqual("T2", generics.genericParameterSecondary()[1].genericParameter().GetText());
//            Assert.AreEqual(">", generics.RGENERIC().GetText());
//        }

//        [TestMethod]
//        public void GenericType_Invalid_1()
//        {
//            string input = "type Test<>{}";
//            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
//        }

//        [TestMethod]
//        public void GenericType_Invalid_1a()
//        {
//            string input = "type Test<{}";
//            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
//        }

//        [TestMethod]
//        public void GenericType_Invalid_1b()
//        {
//            string input = "type Test>{}";
//            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
//        }

//        [TestMethod]
//        public void GenericType_Invalid_2()
//        {
//            string input = "type Test<,>{}";
//            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
//        }

//        [TestMethod]
//        public void GenericType_Invalid_2a()
//        {
//            string input = "type Test<T,>{}";
//            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
//        }

//        [TestMethod]
//        public void GenericType_Invalid_2b()
//        {
//            string input = "type Test<T,{}";
//            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
//        }
//    }
//}
