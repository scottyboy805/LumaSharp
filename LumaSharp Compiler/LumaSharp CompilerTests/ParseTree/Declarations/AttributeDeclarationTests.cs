﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            LumaSharpParser.AttributeReferenceContext attrib = context.attributeReference(0);

            // Check for attributes
            Assert.AreEqual(1, context.attributeReference().Length);
            Assert.AreEqual("#", attrib.HASH().GetText());
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
            LumaSharpParser.AttributeReferenceContext attrib1 = context.attributeReference(0);
            LumaSharpParser.AttributeReferenceContext attrib2 = context.attributeReference(1);

            // Check for attributes
            Assert.AreEqual(2, context.attributeReference().Length);
            Assert.AreEqual("#", attrib1.HASH().GetText());
            Assert.AreEqual("MyAttribute", attrib1.typeReference().GetText());
            Assert.AreEqual("#", attrib2.HASH().GetText());
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
            LumaSharpParser.AttributeReferenceContext attrib = context.attributeReference(0);

            // Check for attributes
            Assert.AreEqual(1, context.attributeReference().Length);
            Assert.AreEqual("#", attrib.HASH().GetText());
            Assert.AreEqual("MyAttribute", attrib.typeReference().GetText());
            Assert.AreEqual("(", attrib.argumentList().LPAREN().GetText());
            Assert.AreEqual(")", attrib.argumentList().RPAREN().GetText());
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
            LumaSharpParser.AttributeReferenceContext attrib = context.attributeReference(0);

            // Check for attributes
            Assert.AreEqual(1, context.attributeReference().Length);
            Assert.AreEqual("#", attrib.HASH().GetText());
            Assert.AreEqual("MyAttribute", attrib.typeReference().GetText());
            Assert.AreEqual("(", attrib.argumentList().LPAREN().GetText());
            Assert.AreEqual("5", attrib.argumentList().expressionList().expression().GetText());
            Assert.AreEqual(")", attrib.argumentList().RPAREN().GetText());
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
            LumaSharpParser.AttributeReferenceContext attrib = context.attributeReference(0);

            // Check for attributes
            Assert.AreEqual(1, context.attributeReference().Length);
            Assert.AreEqual("#", attrib.HASH().GetText());
            Assert.AreEqual("MyAttribute", attrib.typeReference().GetText());
            Assert.AreEqual("(", attrib.argumentList().LPAREN().GetText());
            Assert.AreEqual("5", attrib.argumentList().expressionList().expression().GetText());
            Assert.AreEqual(",", attrib.argumentList().expressionList().expressionSecondary(0).COMMA().GetText());
            Assert.AreEqual("4.5", attrib.argumentList().expressionList().expressionSecondary(0).expression().GetText());
            Assert.AreEqual(")", attrib.argumentList().RPAREN().GetText());
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
            LumaSharpParser.AttributeReferenceContext attrib = context.attributeReference(0);

            // Check for attributes
            Assert.AreEqual(1, context.attributeReference().Length);
            Assert.AreEqual("#", attrib.HASH().GetText());
            Assert.AreEqual("MyAttribute", attrib.typeReference().GetText());
            Assert.AreEqual("(", attrib.argumentList().LPAREN().GetText());
            Assert.AreEqual("5", attrib.argumentList().expressionList().expression().GetText());
            Assert.AreEqual(",", attrib.argumentList().expressionList().expressionSecondary(0).COMMA().GetText());
            Assert.AreEqual("4.5", attrib.argumentList().expressionList().expressionSecondary(0).expression().GetText());
            Assert.AreEqual(",", attrib.argumentList().expressionList().expressionSecondary(1).COMMA().GetText());
            Assert.AreEqual(@"""Hello""", attrib.argumentList().expressionList().expressionSecondary(1).expression().GetText());
            Assert.AreEqual(")", attrib.argumentList().RPAREN().GetText());
        }
    }
}
