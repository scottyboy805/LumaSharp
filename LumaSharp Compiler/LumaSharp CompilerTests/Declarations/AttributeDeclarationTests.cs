using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Declarations
{
    [TestClass]
    public class AttributeDeclarationTests
    {
        [TestMethod]
        public void Attribute()
        {
            string input = "#MyAttribute type Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            // Get attribute
            LumaSharpParser.AttributeDeclarationContext[] attrib = contract.attributeDeclaration();

            // Check for attributes
            Assert.AreEqual(1, attrib.Length);
            Assert.AreEqual("#", attrib[0].GetChild(0).GetText());
            Assert.AreEqual("MyAttribute", attrib[0].IDENTIFIER().GetText());
        }

        [TestMethod]
        public void AttributeMultiple()
        {
            string input = "#MyAttribute #OtherAttribute type Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            // Get attribute
            LumaSharpParser.AttributeDeclarationContext[] attrib = contract.attributeDeclaration();

            // Check for attributes
            Assert.AreEqual(2, attrib.Length);
            Assert.AreEqual("#", attrib[0].GetChild(0).GetText());
            Assert.AreEqual("MyAttribute", attrib[0].IDENTIFIER().GetText());
            Assert.AreEqual("#", attrib[1].GetChild(0).GetText());
            Assert.AreEqual("OtherAttribute", attrib[1].IDENTIFIER().GetText());
        }

        [TestMethod]
        public void Attribute_Parameters_0()
        {
            string input = "#MyAttribute() type Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            // Get attribute
            LumaSharpParser.AttributeDeclarationContext[] attrib = contract.attributeDeclaration();

            // Check for attributes
            Assert.AreEqual(1, attrib.Length);
            Assert.AreEqual("#", attrib[0].GetChild(0).GetText());
            Assert.AreEqual("MyAttribute", attrib[0].IDENTIFIER().GetText());
            Assert.AreEqual("(", attrib[0].GetChild(2).GetText());
            Assert.AreEqual(")", attrib[0].GetChild(3).GetText());
        }

        [TestMethod]
        public void Attribute_Parameters_1()
        {
            string input = "#MyAttribute(5) type Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            // Get attribute
            LumaSharpParser.AttributeDeclarationContext[] attrib = contract.attributeDeclaration();

            // Check for attributes
            Assert.AreEqual(1, attrib.Length);
            Assert.AreEqual("#", attrib[0].GetChild(0).GetText());
            Assert.AreEqual("MyAttribute", attrib[0].IDENTIFIER().GetText());
            Assert.AreEqual("(", attrib[0].GetChild(2).GetText());
            Assert.AreEqual("5", attrib[0].GetChild(3).GetText());
            Assert.AreEqual(")", attrib[0].GetChild(4).GetText());
        }

        [TestMethod]
        public void Attribute_Parameters_2()
        {
            string input = "#MyAttribute(5, 4.5) type Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            // Get attribute
            LumaSharpParser.AttributeDeclarationContext[] attrib = contract.attributeDeclaration();

            // Check for attributes
            Assert.AreEqual(1, attrib.Length);
            Assert.AreEqual("#", attrib[0].GetChild(0).GetText());
            Assert.AreEqual("MyAttribute", attrib[0].IDENTIFIER().GetText());
            Assert.AreEqual("(", attrib[0].GetChild(2).GetText());
            Assert.AreEqual("5", attrib[0].GetChild(3).GetText());
            Assert.AreEqual(",", attrib[0].GetChild(4).GetText());
            Assert.AreEqual("4.5", attrib[0].GetChild(5).GetText());
            Assert.AreEqual(")", attrib[0].GetChild(6).GetText());
        }

        [TestMethod]
        public void Attribute_Parameters_3()
        {
            string input = @"#MyAttribute(5, 4.5, ""Hello"") type Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.TypeDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.TypeDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.TypeDeclarationContext));

            // Check for type name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            // Get attribute
            LumaSharpParser.AttributeDeclarationContext[] attrib = contract.attributeDeclaration();

            // Check for attributes
            Assert.AreEqual(1, attrib.Length);
            Assert.AreEqual("#", attrib[0].GetChild(0).GetText());
            Assert.AreEqual("MyAttribute", attrib[0].IDENTIFIER().GetText());
            Assert.AreEqual("(", attrib[0].GetChild(2).GetText());
            Assert.AreEqual("5", attrib[0].GetChild(3).GetText());
            Assert.AreEqual(",", attrib[0].GetChild(4).GetText());
            Assert.AreEqual("4.5", attrib[0].GetChild(5).GetText());
            Assert.AreEqual(",", attrib[0].GetChild(6).GetText());
            Assert.AreEqual(@"""Hello""", attrib[0].GetChild(7).GetText());
            Assert.AreEqual(")", attrib[0].GetChild(8).GetText());
        }
    }
}
