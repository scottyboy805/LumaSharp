using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.ParseTree.Declarations
{
    [TestClass]
    public class ImportDeclarationTests
    {
        [TestMethod]
        public void Import()
        {
            string input = "import Collections";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ImportElementContext[] imports;
            Assert.IsNotNull(imports = context.importElement());

            // Check for namespace name
            Assert.AreEqual("Collections", imports[0].importStatement().namespaceName().GetText());
        }

        [TestMethod]
        public void ImportNested()
        {
            string input = "import Collections:Generic";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ImportElementContext[] imports;
            Assert.IsNotNull(imports = context.importElement());

            // Check for namespace name
            Assert.AreEqual("Collections", imports[0].importStatement().namespaceName().IDENTIFIER().GetText());
            Assert.AreEqual("Generic", imports[0].importStatement().namespaceName().namespaceNameSecondary(0).IDENTIFIER().GetText());
        }

        [TestMethod]
        public void ImportNestedMultiple()
        {
            string input = "import Collections:Generic:Async";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ImportElementContext[] imports;
            Assert.IsNotNull(imports = context.importElement());

            // Check for namespace name
            Assert.AreEqual("Collections", imports[0].importStatement().namespaceName().IDENTIFIER().GetText());
            Assert.AreEqual("Generic", imports[0].importStatement().namespaceName().namespaceNameSecondary(0).IDENTIFIER().GetText());
            Assert.AreEqual("Async", imports[0].importStatement().namespaceName().namespaceNameSecondary(1).IDENTIFIER().GetText());
        }

        [TestMethod]
        public void ImportMultiple()
        {
            string input = "import Collections import Collections:Generic";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ImportElementContext[] imports;
            Assert.IsNotNull(imports = context.importElement());

            // Check for namespace name
            Assert.AreEqual("Collections", imports[0].importStatement().namespaceName().IDENTIFIER().GetText());

            Assert.AreEqual("Collections", imports[1].importStatement().namespaceName().IDENTIFIER().GetText());
            Assert.AreEqual("Generic", imports[1].importStatement().namespaceName().namespaceNameSecondary(0).IDENTIFIER().GetText());
        }

        [TestMethod]
        public void ImportAlias()
        {
            string input = "import C as Collections.List";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ImportElementContext[] imports;
            Assert.IsNotNull(imports = context.importElement());

            // Check for namespace name
            Assert.AreEqual("C", imports[0].importAlias().IDENTIFIER().GetText());
            Assert.AreEqual("Collections", imports[0].importAlias().namespaceName().IDENTIFIER().GetText());
            Assert.AreEqual("List", imports[0].importAlias().typeReference().GetText());
        }

        [TestMethod]
        public void ImportAliasNested()
        {
            string input = "import C as Collections:Generic.List";
            LumaSharpParser.CompilationUnitContext context = TestUtils.ParseInputString(input);

            // Check for root type
            LumaSharpParser.ImportElementContext[] imports;
            Assert.IsNotNull(imports = context.importElement());

            // Check for namespace name
            Assert.AreEqual("C", imports[0].importAlias().IDENTIFIER().GetText());
            Assert.AreEqual("Collections", imports[0].importAlias().namespaceName().IDENTIFIER().GetText());
            Assert.AreEqual("Generic", imports[0].importAlias().namespaceName().namespaceNameSecondary(0).IDENTIFIER().GetText());
            Assert.AreEqual("List", imports[0].importAlias().typeReference().GetText());
        }
    }
}
