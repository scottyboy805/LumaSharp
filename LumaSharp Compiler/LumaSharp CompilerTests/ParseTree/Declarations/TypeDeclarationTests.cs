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
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void ExportType()
        {
            string input = "export type Test{}";
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void InternalType()
        {
            string input = "internal type Test{}";
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void GenericType_1()
        {
            string input = "type Test<T>{}";
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            LumaSharpParser.GenericParameterListContext generics = context.genericParameterList();

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
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            LumaSharpParser.GenericParameterListContext generics = context.genericParameterList();

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
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            LumaSharpParser.InheritParametersContext inherit = context.inheritParameters();

            Assert.IsNotNull(inherit);
            Assert.AreEqual(2, inherit.ChildCount);
            Assert.AreEqual(":", inherit.GetChild(0).GetText());
            Assert.AreEqual("CBase", inherit.GetChild(1).GetText());
        }

        [TestMethod]
        public void InheritType_2()
        {
            string input = "type Test : CBase1, CBase2{}";
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            LumaSharpParser.InheritParametersContext inherit = context.inheritParameters();

            Assert.IsNotNull(inherit);
            Assert.AreEqual(4, inherit.ChildCount);
            Assert.AreEqual(":", inherit.GetChild(0).GetText());
            Assert.AreEqual("CBase1", inherit.GetChild(1).GetText());
            Assert.AreEqual(",", inherit.GetChild(2).GetText());
            Assert.AreEqual("CBase2", inherit.GetChild(3).GetText());
        }
    }
}
