using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Declarations
{
    [TestClass]
    public class ContractDeclarationTests
    {
        [TestMethod]
        public void Contract()
        {
            string input = "contract Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ContractDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.ContractDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.ContractDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void ExportContract()
        {
            string input = "export contract Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ContractDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.ContractDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.ContractDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void InternalContract()
        {
            string input = "internal contract Test{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ContractDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.ContractDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.ContractDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());
        }

        [TestMethod]
        public void ExportInternalContract_Invalid()
        {
            string input = "export internal contract Test{}";
            Assert.ThrowsException<Exception>(() => TestUtils.ParseInputString(input));
        }

        [TestMethod]
        public void GenericContract_1()
        {
            string input = "contract Test<T>{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ContractDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.ContractDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.ContractDeclarationContext));

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
        public void GenericContract_2()
        {
            string input = "contract Test<T0, T1>{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ContractDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.ContractDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.ContractDeclarationContext));

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
        public void InheritContract_1()
        {
            string input = "contract Test : CBase{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ContractDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.ContractDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.ContractDeclarationContext));

            // Check for namespace name
            Assert.AreEqual("Test", contract.IDENTIFIER().GetText());

            LumaSharpParser.InheritParametersContext inherit = contract.inheritParameters();

            Assert.IsNotNull(inherit);
            Assert.AreEqual(2, inherit.ChildCount);
            Assert.AreEqual(":", inherit.GetChild(0).GetText());
            Assert.AreEqual("CBase", inherit.GetChild(1).GetText());
        }

        [TestMethod]
        public void InheritContract_2()
        {
            string input = "contract Test : CBase1, CBase2{}";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ContractDeclarationContext contract;
            Assert.IsNotNull(contract = context
                .GetChild<LumaSharpParser.ContractDeclarationContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(contract, typeof(LumaSharpParser.ContractDeclarationContext));

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
