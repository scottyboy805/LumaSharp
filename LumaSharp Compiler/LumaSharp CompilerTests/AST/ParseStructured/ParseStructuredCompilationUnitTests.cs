using LumaSharp.Compiler.AST;
using LumaSharp.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.ParseStructured
{
    [TestClass]
    public class ParseStructuredCompilationUnitTests
    {
        [DataTestMethod]
        [DataRow("type MyType{type MySubType{}}", new object[] { new Type[] { typeof(TypeSyntax) } })]
        [DataRow("type MyType{contract MySubType{}}", new object[] { new Type[] { typeof(ContractSyntax) } })]
        [DataRow("type MyType{enum MySubType{}}", new object[] { new Type[] { typeof(EnumSyntax) } })]
        [DataRow("type MyType{i32 myField;}", new object[] { new Type[] { typeof(FieldSyntax) } })]
        [DataRow("type MyType{i32 MyAccessor => 5;}", new object[] { new Type[] { typeof(AccessorSyntax) } })]
        [DataRow("type MyType{i32 MyMethod(){}}", new object[] { new Type[] { typeof(MethodSyntax) } })]
        public void StructuredCompilationUnit_Type(string input, Type[] nestedTypes)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);

            TypeSyntax type = tree.DescendantsOfType<TypeSyntax>().First();
            Assert.IsNotNull(type);

            List<SyntaxNode> descendants = new List<SyntaxNode>(type.Descendants);

            Assert.AreEqual(descendants.Count, nestedTypes.Length);

            for(int i = 0; i < descendants.Count; i++)
            {
                Assert.IsInstanceOfType(descendants[i], nestedTypes[i]);
            }
        }

        [DataTestMethod]
        [DataRow("contract MyContract{type MySubType{}}", new object[] { new Type[] { typeof(TypeSyntax) } })]
        [DataRow("contract MyContract{contract MySubType{}}", new object[] { new Type[] { typeof(ContractSyntax) } })]
        [DataRow("contract MyContract{enum MySubType{}}", new object[] { new Type[] { typeof(EnumSyntax) } })]
        [DataRow("contract MyContract{i32 myField;}", new object[] { new Type[] { typeof(FieldSyntax) } })]
        [DataRow("contract MyContract{i32 MyAccessor => 5;}", new object[] { new Type[] { typeof(AccessorSyntax) } })]
        [DataRow("contract MyContract{i32 MyMethod(){}}", new object[] { new Type[] { typeof(MethodSyntax) } })]
        public void StructuredCompilationUnit_Contract(string input, Type[] nestedTypes)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);

            ContractSyntax contract = tree.DescendantsOfType<ContractSyntax>().First();
            Assert.IsNotNull(contract);

            List<SyntaxNode> descendants = new List<SyntaxNode>(contract.Descendants);

            Assert.AreEqual(descendants.Count, nestedTypes.Length);

            for (int i = 0; i < descendants.Count; i++)
            {
                Assert.IsInstanceOfType(descendants[i], nestedTypes[i]);
            }
        }

        [DataTestMethod]
        [DataRow("enum MyContract{Item}", new object[] { new Type[] { typeof(FieldSyntax) } })]
        [DataRow("enum MyContract{Item1, Item2}", new object[] { new Type[] { typeof(FieldSyntax), typeof(FieldSyntax) } })]
        [DataRow("enum MyContract{Item = 5}", new object[] { new Type[] { typeof(FieldSyntax) } })]
        [DataRow("enum MyContract{Item1 = 5, Item2 = 3 + 4}", new object[] { new Type[] { typeof(FieldSyntax), typeof(FieldSyntax) } })]
        public void StructuredCompilationUnit_Enum(string input, Type[] nestedTypes)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);

            EnumSyntax contract = tree.DescendantsOfType<EnumSyntax>().First();
            Assert.IsNotNull(contract);

            List<SyntaxNode> descendants = new List<SyntaxNode>(contract.Descendants);

            Assert.AreEqual(descendants.Count, nestedTypes.Length);

            for (int i = 0; i < descendants.Count; i++)
            {
                Assert.IsInstanceOfType(descendants[i], nestedTypes[i]);
            }
        }

        [DataTestMethod]
        [DataRow("namespace MyNamespace{type MyType{}}", new object[] { new Type[] { typeof(TypeSyntax) } })]
        [DataRow("namespace MyNamespace{contract MyContract{}}", new object[] { new Type[] { typeof(ContractSyntax) } })]
        [DataRow("namespace MyNamespace{enum MyEnum{}}", new object[] { new Type[] { typeof(EnumSyntax) } })]
        public void StructuredCompilationUnit_Namespace(string input, Type[] nestedTypes)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);

            NamespaceSyntax contract = tree.DescendantsOfType<NamespaceSyntax>().First();
            Assert.IsNotNull(contract);

            List<SyntaxNode> descendants = new List<SyntaxNode>(contract.Descendants);

            Assert.AreEqual(descendants.Count, nestedTypes.Length);

            for (int i = 0; i < descendants.Count; i++)
            {
                Assert.IsInstanceOfType(descendants[i], nestedTypes[i]);
            }
        }
    }
}
