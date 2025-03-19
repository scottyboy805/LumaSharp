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
            LumaSharpParser.EnumDeclarationContext context = TestUtils.ParseEnumDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void EnumTyped_1()
        {
            string input = "enum Test:i8{}";
            LumaSharpParser.EnumDeclarationContext context = TestUtils.ParseEnumDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());
            Assert.AreEqual("i8", context.primitiveType().GetText());
        }

        [TestMethod]
        public void EnumTyped_2()
        {
            string input = "enum Test:u64{}";
            LumaSharpParser.EnumDeclarationContext context = TestUtils.ParseEnumDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());
            Assert.AreEqual("u64", context.primitiveType().GetText());
        }

        [TestMethod]
        public void Enum_WithValue_1()
        {
            string input = "enum Test{Item1}";
            LumaSharpParser.EnumDeclarationContext context = TestUtils.ParseEnumDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            LumaSharpParser.EnumBlockContext fields = context.enumBlock();

            Assert.AreEqual("Item1", fields.enumField(0).GetText());
        }

        [TestMethod]
        public void Enum_WithValue_2()
        {
            string input = "enum Test{Item1, Item2}";
            LumaSharpParser.EnumDeclarationContext context = TestUtils.ParseEnumDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            LumaSharpParser.EnumBlockContext fields = context.enumBlock();

            Assert.AreEqual("Item1", fields.enumField(0).GetText());
            Assert.AreEqual(",", fields.GetChild(2).GetText());
            Assert.AreEqual("Item2", fields.enumField(1).GetText());
        }

        [TestMethod]
        public void Enum_WithValue_Assign_1()
        {
            string input = "enum Test{Item1=5}";
            LumaSharpParser.EnumDeclarationContext context = TestUtils.ParseEnumDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            LumaSharpParser.EnumBlockContext fields = context.enumBlock();

            Assert.AreEqual("Item1", fields.enumField(0).IDENTIFIER().GetText());
            Assert.AreEqual("=", fields.enumField(0).variableAssignment().GetChild(0).GetText());
            Assert.AreEqual("5", fields.enumField(0).variableAssignment().GetChild(1).GetText());
        }

        [TestMethod]
        public void Enum_WithValue_Assign_2()
        {
            string input = "enum Test{Item1=5,Item2=10}";
            LumaSharpParser.EnumDeclarationContext context = TestUtils.ParseEnumDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.EnumDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            LumaSharpParser.EnumBlockContext fields = context.enumBlock();

            Assert.AreEqual("Item1", fields.enumField(0).IDENTIFIER().GetText());
            Assert.AreEqual("=", fields.enumField(0).variableAssignment().GetChild(0).GetText());
            Assert.AreEqual("5", fields.enumField(0).variableAssignment().GetChild(1).GetText());

            Assert.AreEqual(",", fields.GetChild(2).GetText());

            Assert.AreEqual("Item2", fields.enumField(1).IDENTIFIER().GetText());
            Assert.AreEqual("=", fields.enumField(1).variableAssignment().GetChild(0).GetText());
            Assert.AreEqual("10", fields.enumField(1).variableAssignment().GetChild(1).GetText());
        }
    }
}
