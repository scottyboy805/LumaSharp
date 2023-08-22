using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Declarations
{
    [TestClass]
    public class TypeDeclarationTests
    {
        [TestMethod]
        public void Type()
        {
            string input = "type Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext rootType;
            Assert.IsNotNull(rootType = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(rootType, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", rootType.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void ExportType()
        {
            string input = "export type Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext rootType;
            Assert.IsNotNull(rootType = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(rootType, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", rootType.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void InternalType()
        {
            string input = "internal type Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext rootType;
            Assert.IsNotNull(rootType = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(rootType, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", rootType.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void ExportInternalType_Invalid()
        {
            string input = "export internal type Test{}";
            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
        }

        [TestMethod]
        public void TypeMultiple()
        {
            string input = "type Test1{}type Test2{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root namespace
            LumaSharpParser.TypeDeclarationContext type1;
            Assert.IsNotNull(type1 = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for namespace declaration
            Assert.IsInstanceOfType(type1, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test1", type1.IDENTIFIER().GetText());

            // Check for root namespace
            LumaSharpParser.TypeDeclarationContext type2;
            Assert.IsNotNull(type2 = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(1));

            // Check for namespace declaration
            Assert.IsInstanceOfType(type2, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test2", type2.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void GenericType_1()
        {
            string input = "type Test<T>{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            LumaSharpParser.GenericParametersContext generics = contract.genericParameters();

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
            LumaSharpParser.TypeDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            LumaSharpParser.GenericParametersContext generics = contract.genericParameters();

            Assert.IsNotNull(generics);
            Assert.AreEqual(5, generics.ChildCount);
            Assert.AreEqual("<", generics.GetChild(0).GetText());
            Assert.AreEqual("T0", generics.GetChild(1).GetText());
            Assert.AreEqual(",", generics.GetChild(2).GetText());
            Assert.AreEqual("T1", generics.GetChild(3).GetText());
            Assert.AreEqual(">", generics.GetChild(4).GetText());
        }

        [TestMethod]
        public void InheritType_1()
        {
            string input = "type Test : CBase{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            LumaSharpParser.InheritParametersContext inherit = contract.inheritParameters();

            Assert.IsNotNull(inherit);
            Assert.AreEqual(2, inherit.ChildCount);
            Assert.AreEqual(":", inherit.GetChild(0).GetText());
            Assert.AreEqual("CBase", inherit.GetChild(1).GetText());
        }

        [TestMethod]
        public void InheritType_2()
        {
            string input = "type Test : CBase1, CBase2{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            LumaSharpParser.InheritParametersContext inherit = contract.inheritParameters();

            Assert.IsNotNull(inherit);
            Assert.AreEqual(4, inherit.ChildCount);
            Assert.AreEqual(":", inherit.GetChild(0).GetText());
            Assert.AreEqual("CBase1", inherit.GetChild(1).GetText());
            Assert.AreEqual(",", inherit.GetChild(2).GetText());
            Assert.AreEqual("CBase2", inherit.GetChild(3).GetText());
        }
    }
}
