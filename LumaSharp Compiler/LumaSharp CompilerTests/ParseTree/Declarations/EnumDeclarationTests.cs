using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Declarations
{
    [TestClass]
    public class EnumDeclarationTests
    {
        [TestMethod]
        public void Enum()
        {
            string input = "enum Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.EnumDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.EnumDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void EnumTyped_1()
        {
            string input = "enum Test:i8{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.EnumDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.EnumDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());
            Assert.AreEqual("i8", contract.primitiveType().GetText());
        }

        [TestMethod]
        public void EnumTyped_2()
        {
            string input = "enum Test:u64{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.EnumDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.EnumDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());
            Assert.AreEqual("u64", contract.primitiveType().GetText());
        }

        [TestMethod]
        public void Enum_WithValue_1()
        {
            string input = "enum Test{Item1}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.EnumDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.EnumDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            LumaSharpParser.FieldBlockContext fields = contract.fieldBlock();

            Assert.AreEqual(1, fields.ChildCount);
            Assert.AreEqual("Item1", fields.GetChild(0).GetText());
        }

        [TestMethod]
        public void Enum_WithValue_2()
        {
            string input = "enum Test{Item1, Item2}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.EnumDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.EnumDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            LumaSharpParser.FieldBlockContext fields = contract.fieldBlock();

            Assert.AreEqual(3, fields.ChildCount);
            Assert.AreEqual("Item1", fields.GetChild(0).GetText());
            Assert.AreEqual(",", fields.GetChild(1).GetText());
            Assert.AreEqual("Item2", fields.GetChild(2).GetText());
        }

        [TestMethod]
        public void Enum_WithValue_Assign_1()
        {
            string input = "enum Test{Item1=5}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.EnumDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.EnumDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            LumaSharpParser.FieldBlockContext fields = contract.fieldBlock();

            Assert.AreEqual(1, fields.ChildCount);
            Assert.AreEqual("Item1", fields.GetChild(0).GetChild(0).GetText());
            Assert.AreEqual("=", fields.GetChild(0).GetChild(1).GetText());
            Assert.AreEqual("5", fields.GetChild(0).GetChild(2).GetText());
        }

        [TestMethod]
        public void Enum_WithValue_Assign_2()
        {
            string input = "enum Test{Item1=5,Item2=10}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.EnumDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.EnumDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            LumaSharpParser.FieldBlockContext fields = contract.fieldBlock();

            Assert.AreEqual(3, fields.ChildCount);
            Assert.AreEqual("Item1", fields.GetChild(0).GetChild(0).GetText());
            Assert.AreEqual("=", fields.GetChild(0).GetChild(1).GetText());
            Assert.AreEqual("5", fields.GetChild(0).GetChild(2).GetText());

            Assert.AreEqual(",", fields.GetChild(1).GetText());

            Assert.AreEqual("Item2", fields.GetChild(2).GetChild(0).GetText());
            Assert.AreEqual("=", fields.GetChild(2).GetChild(1).GetText());
            Assert.AreEqual("10", fields.GetChild(2).GetChild(2).GetText());
        }
    }
}
