using LumaSharp_Compiler;
using LumaSharp_Compiler.AST;
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
        [DataRow("import MyNamespace;")]
        public void CompilationUnit_ImportStatement_1(string input)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);
            Assert.IsNotNull(tree.DescendantsOfType<ImportSyntax>().First());
        }

        [DataTestMethod]
        [DataRow("import MyNamespace:MyNestedNamespace;")]
        public void CompilationUnit_ImportStatement_2(string input)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);
            Assert.IsNotNull(tree.DescendantsOfType<ImportSyntax>().First());
        }

        [DataTestMethod]
        [DataRow("import Test as MyNamespace.SomeType;")]
        public void CompilationUnit_ImportAlias_1(string input)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);
            Assert.IsNotNull(tree.DescendantsOfType<ImportSyntax>().First());
            Assert.IsTrue(tree.DescendantsOfType<ImportSyntax>().First().HasAlias);
        }

        [DataTestMethod]
        [DataRow("import Test as MyNamespace:SubNamespace.SomeType;")]
        public void CompilationUnit_ImportAlias_2(string input)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);
            Assert.IsNotNull(tree.DescendantsOfType<ImportSyntax>().First());
            Assert.IsTrue(tree.DescendantsOfType<ImportSyntax>().First().HasAlias);
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

        [DataTestMethod]
        [DataRow("namespace MyNamespace{}")]
        public void CompilationUnit_Namespace_1(string input)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);
            Assert.IsNotNull(tree.DescendantsOfType<NamespaceSyntax>().First());
        }

        [DataTestMethod]
        [DataRow("namespace MyNamespace:MyNestedNamespace{}")]
        public void CompilationUnit_Namespace_2(string input)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);
            Assert.IsNotNull(tree.DescendantsOfType<NamespaceSyntax>().First());
        }

        [DataTestMethod]
        [DataRow("import MyNamespace;type MyType{}", new object[] { new Type[] {
            typeof(ImportSyntax), typeof(TypeSyntax) }})]
        [DataRow("import MyNamespace;contract MyContract{}", new object[] { new Type[] {
            typeof(ImportSyntax), typeof(ContractSyntax) }})]
        [DataRow("import MyNamespace;enum MyType{}", new object[] { new Type[] {
            typeof(ImportSyntax), typeof(EnumSyntax) }})]
        [DataRow("import MyNamespace;namespace MyNamespace{}", new object[] { new Type[] {
            typeof(ImportSyntax), typeof(NamespaceSyntax) }})]
        public void CompilationUnit_Combined_2(string input, Type[] types)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(2, tree.RootElementCount);
            Assert.IsNotNull(tree.DescendantsOfType(types[0]).First());
            Assert.IsNotNull(tree.DescendantsOfType(types[1]).First());
        }
    }
}
