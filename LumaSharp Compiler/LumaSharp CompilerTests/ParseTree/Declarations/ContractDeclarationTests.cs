//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace LumaSharp_CompilerTests.ParseTree.Declarations
//{
//    [TestClass]
//    public class ContractDeclarationTests
//    {
//        [TestMethod]
//        public void Contract()
//        {
//            string input = "contract Test{}";
//            LumaSharpParser.ContractDeclarationContext context = TestUtils.ParseContractDeclaration(input);

//            // Check for root type
//            Assert.IsNotNull(context);

//            // Check for type declaration
//            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.ContractDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test", context.IDENTIFIER().GetText());
//        }

//        [TestMethod]
//        public void ExportContract()
//        {
//            string input = "export contract Test{}";
//            LumaSharpParser.ContractDeclarationContext context = TestUtils.ParseContractDeclaration(input);

//            // Check for root type
//            Assert.IsNotNull(context);

//            // Check for type declaration
//            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.ContractDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test", context.IDENTIFIER().GetText());
//        }

//        [TestMethod]
//        public void InternalContract()
//        {
//            string input = "internal contract Test{}";
//            LumaSharpParser.ContractDeclarationContext context = TestUtils.ParseContractDeclaration(input);

//            // Check for root type
//            Assert.IsNotNull(context);

//            // Check for type declaration
//            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.ContractDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test", context.IDENTIFIER().GetText());
//        }

//        [TestMethod]
//        public void GenericContract_1()
//        {
//            string input = "contract Test<T>{}";
//            LumaSharpParser.ContractDeclarationContext context = TestUtils.ParseContractDeclaration(input);

//            // Check for root type
//            Assert.IsNotNull(context);

//            // Check for type declaration
//            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.ContractDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

//            LumaSharpParser.GenericParameterListContext generics = context.genericParameterList();

//            Assert.IsNotNull(generics);
//            Assert.AreEqual("<", generics.LGENERIC().GetText());
//            Assert.AreEqual("T", generics.genericParameter().GetText());
//            Assert.AreEqual(">", generics.RGENERIC().GetText());
//        }

//        [TestMethod]
//        public void GenericContract_2()
//        {
//            string input = "contract Test<T0, T1>{}";
//            LumaSharpParser.ContractDeclarationContext context = TestUtils.ParseContractDeclaration(input);

//            // Check for root type
//            Assert.IsNotNull(context);

//            // Check for type declaration
//            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.ContractDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

//            LumaSharpParser.GenericParameterListContext generics = context.genericParameterList();

//            Assert.IsNotNull(generics);
//            Assert.AreEqual("<", generics.LGENERIC().GetText());
//            Assert.AreEqual("T0", generics.genericParameter().GetText());
//            Assert.AreEqual(",", generics.genericParameterSecondary(0).COMMA().GetText());
//            Assert.AreEqual("T1", generics.genericParameterSecondary(0).genericParameter().GetText());
//            Assert.AreEqual(">", generics.RGENERIC().GetText());
//        }

//        [TestMethod]
//        public void InheritContract_1()
//        {
//            string input = "contract Test : CBase{}";
//            LumaSharpParser.ContractDeclarationContext context = TestUtils.ParseContractDeclaration(input);

//            // Check for root type
//            Assert.IsNotNull(context);

//            // Check for type declaration
//            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.ContractDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

//            LumaSharpParser.InheritParametersContext inherit = context.inheritParameters();

//            Assert.IsNotNull(inherit);
//            Assert.AreEqual(":", inherit.COLON().GetText());
//            Assert.AreEqual("CBase", inherit.typeReferenceList().typeReference().GetText());
//        }

//        [TestMethod]
//        public void InheritContract_2()
//        {
//            string input = "contract Test : CBase1, CBase2{}";
//            LumaSharpParser.ContractDeclarationContext context = TestUtils.ParseContractDeclaration(input);

//            // Check for root type
//            Assert.IsNotNull(context);

//            // Check for type declaration
//            Assert.IsInstanceOfType(context, typeof(LumaSharpParser.ContractDeclarationContext));

//            // Check for namespace name
//            Assert.AreEqual("Test", context.IDENTIFIER().GetText());

//            LumaSharpParser.InheritParametersContext inherit = context.inheritParameters();

//            Assert.IsNotNull(inherit);
//            Assert.AreEqual(":", inherit.COLON().GetText());
//            Assert.AreEqual("CBase1", inherit.typeReferenceList().typeReference().GetText());
//            Assert.AreEqual(",", inherit.typeReferenceList().typeReferenceSecondary(0).COMMA().GetText());
//            Assert.AreEqual("CBase2", inherit.typeReferenceList().typeReferenceSecondary(0).typeReference().GetText());
//        }
//    }
//}
