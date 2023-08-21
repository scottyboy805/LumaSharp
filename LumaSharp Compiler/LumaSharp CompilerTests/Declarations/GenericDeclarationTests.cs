using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Declarations
{
    [TestClass]
    public class GenericDeclarationTests
    {
        [TestMethod]
        public void GenericType_1()
        {
            string input = "type Test<T>{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext rootType;
            Assert.IsNotNull(rootType = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(rootType, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", rootType.IDENTIFIER().GetText());

            LumaSharpParser.GenericParametersContext generics = rootType.genericParameters();

            Assert.IsNotNull(generics);
            Assert.AreEqual(3, generics.ChildCount);
            Assert.AreEqual("<", generics.GetChild(0).GetText());
            Assert.AreEqual("T", generics.GetChild(1).GetText());
            Assert.AreEqual(">", generics.GetChild(2).GetText());
        }

        [TestMethod]
        public void GenericType_2()
        {
            string input = "type Test<T0, T1>{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext rootType;
            Assert.IsNotNull(rootType = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(rootType, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", rootType.IDENTIFIER().GetText());

            LumaSharpParser.GenericParametersContext generics = rootType.genericParameters();

            Assert.IsNotNull(generics);
            Assert.AreEqual(5, generics.ChildCount);
            Assert.AreEqual("<", generics.GetChild(0).GetText());
            Assert.AreEqual("T0", generics.GetChild(1).GetText());
            Assert.AreEqual(",", generics.GetChild(2).GetText());
            Assert.AreEqual("T1", generics.GetChild(3).GetText());
            Assert.AreEqual(">", generics.GetChild(4).GetText());
        }

        [TestMethod]
        public void GenericType_3()
        {
            string input = "type Test<T0, T1, T2>{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext rootType;
            Assert.IsNotNull(rootType = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(rootType, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", rootType.IDENTIFIER().GetText());

            LumaSharpParser.GenericParametersContext generics = rootType.genericParameters();

            Assert.IsNotNull(generics);
            Assert.AreEqual(7, generics.ChildCount);
            Assert.AreEqual("<", generics.GetChild(0).GetText());
            Assert.AreEqual("T0", generics.GetChild(1).GetText());
            Assert.AreEqual(",", generics.GetChild(2).GetText());
            Assert.AreEqual("T1", generics.GetChild(3).GetText());
            Assert.AreEqual(",", generics.GetChild(4).GetText());
            Assert.AreEqual("T2", generics.GetChild(5).GetText());
            Assert.AreEqual(">", generics.GetChild(6).GetText());
        }

        [TestMethod]
        public void GenericType_Invalid_1()
        {
            string input = "type Test<>{}";
            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
        }

        [TestMethod]
        public void GenericType_Invalid_1a()
        {
            string input = "type Test<{}";
            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
        }

        [TestMethod]
        public void GenericType_Invalid_1b()
        {
            string input = "type Test>{}";
            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
        }

        [TestMethod]
        public void GenericType_Invalid_2()
        {
            string input = "type Test<,>{}";
            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
        }

        [TestMethod]
        public void GenericType_Invalid_2a()
        {
            string input = "type Test<T,>{}";
            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
        }

        [TestMethod]
        public void GenericType_Invalid_2b()
        {
            string input = "type Test<T,{}";
            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
        }
    }
}
