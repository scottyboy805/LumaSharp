using LumaSharp_Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST
{
    [TestClass]
    public class TypeSyntaxTests
    {
        [DataTestMethod]
        [DataRow("type MyType{}", "MyType")]
        [DataRow("type _MyType{}", "_MyType")]
        [DataRow("type MyType123{}", "MyType123")]
        [DataRow("type _MyType123{}", "_MyType123")]
        public void UserType(string input, string identifier)
        {
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Build AST node
            TypeSyntax syntax = new TypeSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsFalse(syntax.HasAccessModifiers);
            Assert.IsFalse(syntax.HasGenericParameters);
            Assert.IsFalse(syntax.HasBaseTypes);
            Assert.IsFalse(syntax.HasMembers);
        }

        [DataTestMethod]
        [DataRow("export type MyType{}", "MyType", 1)]
        [DataRow("internal type _MyType{}", "_MyType", 1)]
        [DataRow("hidden type MyType123{}", "MyType123", 1)]
        [DataRow("global export type MyType{}", "MyType", 2)]
        [DataRow("global internal type _MyType{}", "_MyType", 2)]
        [DataRow("global hidden type MyType123{}", "MyType123", 2)]
        public void UserType_AccessModifier(string input, string identifier, int modifierCount)
        {
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Build AST node
            TypeSyntax syntax = new TypeSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsTrue(syntax.HasAccessModifiers);
            Assert.AreEqual(modifierCount, syntax.AccessModifierCount);
            Assert.IsFalse(syntax.HasGenericParameters);
            Assert.IsFalse(syntax.HasBaseTypes);
            Assert.IsFalse(syntax.HasMembers);
        }

        [DataTestMethod]
        [DataRow("type MyType<T>{}", "MyType", 1)]
        [DataRow("type _MyType<T0, T1>{}", "_MyType", 2)]
        [DataRow("type MyType123<T0, T1, T2>{}", "MyType123", 3)]
        [DataRow("type _MyType123<T0, T1, T2, T3>{}", "_MyType123", 4)]
        public void UserType_Generic(string input, string identifier, int genericCount)
        {
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Build AST node
            TypeSyntax syntax = new TypeSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsFalse(syntax.HasAccessModifiers);
            Assert.IsTrue(syntax.HasGenericParameters);
            Assert.AreEqual(genericCount, syntax.GenericParameterCount);
            Assert.IsFalse(syntax.HasBaseTypes);
            Assert.IsFalse(syntax.HasMembers);
        }


        [DataTestMethod]
        [DataRow("type MyType : MyBase{}", "MyType", 1)]
        [DataRow("type _MyType : MyBase, MyBase2{}", "_MyType", 2)]
        [DataRow("type MyType123 : MyBase, MyBase2, MyBase3{}", "MyType123", 3)]
        [DataRow("type MyType : MyBase<T>{}", "MyType", 1)]
        [DataRow("type MyType : MyBase<i8>{}", "MyType", 1)]
        public void UserType_BaseType(string input, string identifier, int baseCount)
        {
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Build AST node
            TypeSyntax syntax = new TypeSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsFalse(syntax.HasAccessModifiers);
            Assert.IsFalse(syntax.HasGenericParameters);
            Assert.IsTrue(syntax.HasBaseTypes);
            Assert.AreEqual(baseCount, syntax.BaseTypeCount);
            Assert.IsFalse(syntax.HasMembers);
        }

        [DataTestMethod]
        [DataRow("type MyType{type A{}}", "MyType", 1)]
        [DataRow("type _MyType{type A{} contract B{}}", "_MyType", 2)]
        [DataRow("type MyType123{type A{} contract B{} i32 field;}", "MyType123", 3)]
        [DataRow("type _MyType123{type A{} contract B{} i32 field; i32 accessor => 5;}", "_MyType123", 4)]
        [DataRow("type _MyType123{type A{} contract B{} i32 field; i32 accessor => 5; i32 method() { return 5;}}", "_MyType123", 5)]
        public void UserType_Member(string input, string identifier, int memberCount)
        {
            LumaSharpParser.TypeDeclarationContext context = TestUtils.ParseTypeDeclaration(input);

            // Build AST node
            TypeSyntax syntax = new TypeSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsFalse(syntax.HasAccessModifiers);
            Assert.IsFalse(syntax.HasGenericParameters);
            Assert.IsFalse(syntax.HasBaseTypes);
            Assert.IsTrue(syntax.HasMembers);
            Assert.AreEqual(memberCount, syntax.MemberCount);
        }
    }
}
