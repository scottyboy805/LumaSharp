using LumaSharp_Compiler;
using LumaSharp_Compiler.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST.Parse
{
    [TestClass]
    public class ParseCompilationUnitTests
    {
        [DataTestMethod]
        [DataRow(" ")]
        public void CompilationUnit_Empty(string input)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(0, tree.RootElementCount);
        }

        [DataTestMethod]
        [DataRow("type MyType{}")]
        public void CompilationUnit_Type(string input)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);
            Assert.IsNotNull(tree.DescendantsOfType<TypeSyntax>().First());
        }

        [DataTestMethod]
        [DataRow("contract MyType{}")]
        public void CompilationUnit_Contract(string input)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);
            Assert.IsNotNull(tree.DescendantsOfType<ContractSyntax>().First());
        }

        [DataTestMethod]
        [DataRow("enum MyType{}")]
        public void CompilationUnit_Enum(string input)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);
            Assert.IsNotNull(tree.DescendantsOfType<EnumSyntax>().First());
        }
    }
}
