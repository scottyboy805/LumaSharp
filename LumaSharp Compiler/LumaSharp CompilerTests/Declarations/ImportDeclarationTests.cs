using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Declarations
{
    [TestClass]
    public class ImportDeclarationTests
    {
        [TestMethod]
        public void Import()
        {
            string input = "import Collections;";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ImportStatementContext import;
            Assert.IsNotNull(import = context
                .GetChild<LumaSharpParser.ImportStatementContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(import, typeof(LumaSharpParser.ImportStatementContext));

            // Check for namespace name
            Assert.AreEqual("Collections", import.GetChild(1).GetText());
        }

        [TestMethod]
        public void ImportNested()
        {
            string input = "import Collections.Generic;";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ImportStatementContext import;
            Assert.IsNotNull(import = context
                .GetChild<LumaSharpParser.ImportStatementContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(import, typeof(LumaSharpParser.ImportStatementContext));

            // Check for namespace name
            Assert.AreEqual("Collections", import.GetChild(1).GetText());
            Assert.AreEqual(".", import.GetChild(2).GetText());
            Assert.AreEqual("Generic", import.GetChild(3).GetText());
        }

        [TestMethod]
        public void ImportNestedMultiple()
        {
            string input = "import Collections.Generic.Async;";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ImportStatementContext import;
            Assert.IsNotNull(import = context
                .GetChild<LumaSharpParser.ImportStatementContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(import, typeof(LumaSharpParser.ImportStatementContext));

            // Check for namespace name
            Assert.AreEqual("Collections", import.GetChild(1).GetText());
            Assert.AreEqual(".", import.GetChild(2).GetText());
            Assert.AreEqual("Generic", import.GetChild(3).GetText());
            Assert.AreEqual(".", import.GetChild(4).GetText());
            Assert.AreEqual("Async", import.GetChild(5).GetText());
        }

        [TestMethod]
        public void ImportMultiple()
        {
            string input = "import Collections; import Collections.Generic;";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ImportStatementContext import;
            Assert.IsNotNull(import = context
                .GetChild<LumaSharpParser.ImportStatementContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(import, typeof(LumaSharpParser.ImportStatementContext));

            // Get imports
            LumaSharpParser.ImportStatementContext[] imports = context.importStatement(); 

            // Check for namespace name
            Assert.AreEqual("Collections", imports[0].GetChild(1).GetText());

            Assert.AreEqual("Collections", imports[1].GetChild(1).GetText());
            Assert.AreEqual(".", imports[1].GetChild(2).GetText());
            Assert.AreEqual("Generic", imports[1].GetChild(3).GetText());
        }

        [TestMethod]
        public void ImportAlias()
        {
            string input = "import C as Collections.List;";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ImportAliasContext import;
            Assert.IsNotNull(import = context
                .GetChild<LumaSharpParser.ImportAliasContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(import, typeof(LumaSharpParser.ImportAliasContext));

            // Check for namespace name
            Assert.AreEqual("C", import.GetChild(1).GetText());
            Assert.AreEqual("Collections", import.GetChild(3).GetText());
            Assert.AreEqual(".", import.GetChild(4).GetText());
            Assert.AreEqual("List", import.GetChild(5).GetText());
        }

        [TestMethod]
        public void ImportAliasNested()
        {
            string input = "import C as Collections.Generic.List;";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ImportAliasContext import;
            Assert.IsNotNull(import = context
                .GetChild<LumaSharpParser.ImportAliasContext>(0));

            // Check for type declaration
            Assert.IsInstanceOfType(import, typeof(LumaSharpParser.ImportAliasContext));

            // Check for namespace name
            Assert.AreEqual("C", import.GetChild(1).GetText());
            Assert.AreEqual("Collections", import.GetChild(3).GetText());
            Assert.AreEqual(".", import.GetChild(4).GetText());
            Assert.AreEqual("Generic", import.GetChild(5).GetText());
            Assert.AreEqual(".", import.GetChild(6).GetText());
            Assert.AreEqual("List", import.GetChild(7).GetText());
        }
    }
}
