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
            Assert.AreEqual("<", generics.LGENERIC().GetText());
            Assert.AreEqual("T", generics.genericParameter().GetText());
            Assert.AreEqual(">", generics.RGENERIC().GetText());
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
            Assert.AreEqual("<", generics.LGENERIC().GetText());
            Assert.AreEqual("T0", generics.genericParameter().GetText());
            Assert.AreEqual(",", generics.genericParameterSecondary()[0].COMMA().GetText());
            Assert.AreEqual("T1", generics.genericParameterSecondary()[0].genericParameter().GetText());
            Assert.AreEqual(">", generics.RGENERIC().GetText());
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
            Assert.AreEqual(":", inherit.COLON().GetText());
            Assert.AreEqual("CBase", inherit.typeReferenceList().typeReference().GetText());
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
            Assert.AreEqual(":", inherit.COLON().GetText());
            Assert.AreEqual("CBase1", inherit.typeReferenceList().typeReference().GetText());
            Assert.AreEqual(",", inherit.typeReferenceList().typeReferenceSecondary()[0].COMMA().GetText());
            Assert.AreEqual("CBase2", inherit.typeReferenceList().typeReferenceSecondary()[0].typeReference().GetText());
        }
    }
}
