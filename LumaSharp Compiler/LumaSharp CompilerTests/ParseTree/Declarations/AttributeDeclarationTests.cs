using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Declarations
{
    [TestClass]
    public class AttributeDeclarationTests
    {
        [TestMethod]
        public void Attribute()
        {
            string input = "#MyAttribute type Test{}";
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);
            
            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            // Get attribute
            LumaSharpParser.AttributeDeclarationContext attrib = context.attributeDeclaration(0);

            // Check for attributes
            Assert.AreEqual(1, context.attributeDeclaration().Length);
            Assert.AreEqual("#", attrib.GetChild(0).GetText());
            Assert.AreEqual("MyAttribute", attrib.typeReference().GetText());
        }

        [TestMethod]
        public void AttributeMultiple()
        {
            string input = "#MyAttribute #OtherAttribute type Test{}";
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            // Get attribute
            LumaSharpParser.AttributeDeclarationContext attrib1 = context.attributeDeclaration(0);
            LumaSharpParser.AttributeDeclarationContext attrib2 = context.attributeDeclaration(1);

            // Check for attributes
            Assert.AreEqual(2, context.attributeDeclaration().Length);
            Assert.AreEqual("#", attrib1.GetChild(0).GetText());
            Assert.AreEqual("MyAttribute", attrib1.typeReference().GetText());
            Assert.AreEqual("#", attrib2.GetChild(0).GetText());
            Assert.AreEqual("OtherAttribute", attrib2.typeReference().GetText());
        }

        [TestMethod]
        public void Attribute_Parameters_0()
        {
            string input = "#MyAttribute() type Test{}";
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            // Get attribute
            LumaSharpParser.AttributeDeclarationContext attrib = context.attributeDeclaration(0);

            // Check for attributes
            Assert.AreEqual(1, context.attributeDeclaration().Length);
            Assert.AreEqual("#", attrib.GetChild(0).GetText());
            Assert.AreEqual("MyAttribute", attrib.typeReference().GetText());
            Assert.AreEqual("(", attrib.GetChild(2).GetText());
            Assert.AreEqual(")", attrib.GetChild(3).GetText());
        }

        [TestMethod]
        public void Attribute_Parameters_1()
        {
            string input = "#MyAttribute(5) type Test{}";
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            // Get attribute
            LumaSharpParser.AttributeDeclarationContext attrib = context.attributeDeclaration(0);

            // Check for attributes
            Assert.AreEqual(1, context.attributeDeclaration().Length);
            Assert.AreEqual("#", attrib.GetChild(0).GetText());
            Assert.AreEqual("MyAttribute", attrib.typeReference().GetText());
            Assert.AreEqual("(", attrib.GetChild(2).GetText());
            Assert.AreEqual("5", attrib.GetChild(3).GetText());
            Assert.AreEqual(")", attrib.GetChild(4).GetText());
        }

        [TestMethod]
        public void Attribute_Parameters_2()
        {
            string input = "#MyAttribute(5, 4.5) type Test{}";
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            // Get attribute
            LumaSharpParser.AttributeDeclarationContext attrib = context.attributeDeclaration(0);

            // Check for attributes
            Assert.AreEqual(1, context.attributeDeclaration().Length);
            Assert.AreEqual("#", attrib.GetChild(0).GetText());
            Assert.AreEqual("MyAttribute", attrib.typeReference().GetText());
            Assert.AreEqual("(", attrib.GetChild(2).GetText());
            Assert.AreEqual("5", attrib.expression(0).GetText());
            Assert.AreEqual(",", attrib.GetChild(4).GetText());
            Assert.AreEqual("4.5", attrib.expression(1).GetText());
            Assert.AreEqual(")", attrib.GetChild(6).GetText());
        }

        [TestMethod]
        public void Attribute_Parameters_3()
        {
            string input = @"#MyAttribute(5, 4.5, ""Hello"") type Test{}";
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Check for root type
            Assert.IsNotNull(context);

            // Check for type declaration
            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

            // Get attribute
            LumaSharpParser.AttributeDeclarationContext attrib = context.attributeDeclaration(0);

            // Check for attributes
            Assert.AreEqual(1, context.attributeDeclaration().Length);
            Assert.AreEqual("#", attrib.GetChild(0).GetText());
            Assert.AreEqual("MyAttribute", attrib.typeReference().GetText());
            Assert.AreEqual("(", attrib.GetChild(2).GetText());
            Assert.AreEqual("5", attrib.expression(0).GetText());
            Assert.AreEqual(",", attrib.GetChild(4).GetText());
            Assert.AreEqual("4.5", attrib.expression(1).GetText());
            Assert.AreEqual(",", attrib.GetChild(6).GetText());
            Assert.AreEqual(@"""Hello""", attrib.expression(2).GetText());
            Assert.AreEqual(")", attrib.GetChild(8).GetText());
        }
    }
}
