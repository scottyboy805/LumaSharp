using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST
{
    [TestClass]
    public class TypeReferenceSyntaxTests
    {
        #region Primitive
        [DataTestMethod]
        [DataRow("any")]
        [DataRow("i8")]
        [DataRow("u8")]
        [DataRow("i16")]
        [DataRow("u16")]
        [DataRow("i32")]
        [DataRow("u32")]
        [DataRow("i64")]
        [DataRow("u64")]
        [DataRow("float")]
        [DataRow("double")]
        [DataRow("bool")]
        [DataRow("char")]
        public void Primitive(string input)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(input, syntax.Identifier.Text);
            Assert.IsTrue(syntax.IsPrimitiveType);
            Assert.IsFalse(syntax.HasNamespace);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsFalse(syntax.IsArrayType);
        }

        [DataTestMethod]
        [DataRow("any[]")]
        [DataRow("i8[]")]
        [DataRow("u8[]")]
        [DataRow("i16[]")]
        [DataRow("u16[]")]
        [DataRow("i32[]")]
        [DataRow("u32[]")]
        [DataRow("i64[]")]
        [DataRow("u64[]")]
        [DataRow("float[]")]
        [DataRow("double[]")]
        [DataRow("bool[]")]
        [DataRow("char[]")]
        public void Primitive_Array_1(string input)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(input.TrimEnd('[',']'), syntax.Identifier.Text);
            Assert.IsTrue(syntax.IsPrimitiveType);
            Assert.IsFalse(syntax.HasNamespace);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsTrue(syntax.IsArrayType);
            Assert.AreEqual(1, syntax.ArrayParameterRank);
        }

        [DataTestMethod]
        [DataRow("any[,]")]
        [DataRow("i8[,]")]
        [DataRow("u8[,]")]
        [DataRow("i16[,]")]
        [DataRow("u16[,]")]
        [DataRow("i32[,]")]
        [DataRow("u32[,]")]
        [DataRow("i64[,]")]
        [DataRow("u64[,]")]
        [DataRow("float[,]")]
        [DataRow("double[,]")]
        [DataRow("bool[,]")]
        [DataRow("char[,]")]
        public void Primitive_Array_2(string input)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(input.TrimEnd('[', ']', ','), syntax.Identifier.Text);
            Assert.IsTrue(syntax.IsPrimitiveType);
            Assert.IsFalse(syntax.HasNamespace);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsTrue(syntax.IsArrayType);
            Assert.AreEqual(2, syntax.ArrayParameterRank);
        }

        [DataTestMethod]
        [DataRow("any[,,]")]
        [DataRow("i8[,,]")]
        [DataRow("u8[,,]")]
        [DataRow("i16[,,]")]
        [DataRow("u16[,,]")]
        [DataRow("i32[,,]")]
        [DataRow("u32[,,]")]
        [DataRow("i64[,,]")]
        [DataRow("u64[,,]")]
        [DataRow("float[,,]")]
        [DataRow("double[,,]")]
        [DataRow("bool[,,]")]
        [DataRow("char[,,]")]
        public void Primitive_Array_3(string input)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(input.TrimEnd('[', ']', ','), syntax.Identifier.Text);
            Assert.IsTrue(syntax.IsPrimitiveType);
            Assert.IsFalse(syntax.HasNamespace);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsTrue(syntax.IsArrayType);
            Assert.AreEqual(3, syntax.ArrayParameterRank);
        }

        [DataTestMethod]
        [DataRow("any[,]&")]
        [DataRow("i8[,]&")]
        [DataRow("u8[,]&")]
        [DataRow("i16[,]&")]
        [DataRow("u16[,]&")]
        [DataRow("i32[,]&")]
        [DataRow("u32[,]&")]
        [DataRow("i64[,]&")]
        [DataRow("u64[,]&")]
        [DataRow("float[,]&")]
        [DataRow("double[,]&")]
        [DataRow("bool[,]&")]
        [DataRow("char[,]&")]
        public void Primitive_Array_2_Ref(string input)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(input.TrimEnd('[', ']', ',', '&'), syntax.Identifier.Text);
            Assert.IsTrue(syntax.IsPrimitiveType);
            Assert.IsFalse(syntax.HasNamespace);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsTrue(syntax.IsArrayType);
            Assert.AreEqual(2, syntax.ArrayParameterRank);
        }

        [DataTestMethod]
        [DataRow("any[,,]&")]
        [DataRow("i8[,,]&")]
        [DataRow("u8[,,]&")]
        [DataRow("i16[,,]&")]
        [DataRow("u16[,,]&")]
        [DataRow("i32[,,]&")]
        [DataRow("u32[,,]&")]
        [DataRow("i64[,,]&")]
        [DataRow("u64[,,]&")]
        [DataRow("float[,,]&")]
        [DataRow("double[,,]&")]
        [DataRow("bool[,,]&")]
        [DataRow("char[,,]&")]
        public void Primitive_Array_3_Ref(string input)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(input.TrimEnd('[', ']', ',', '&'), syntax.Identifier.Text);
            Assert.IsTrue(syntax.IsPrimitiveType);
            Assert.IsFalse(syntax.HasNamespace);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsTrue(syntax.IsArrayType);
            Assert.AreEqual(3, syntax.ArrayParameterRank);
        }
        #endregion

        #region UserType
        [DataTestMethod]
        [DataRow("MyType")]
        [DataRow("_MyType")]
        [DataRow("MyType123")]
        [DataRow("_MyType123")]
        public void UserType(string input)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(input, syntax.Identifier.Text);
            Assert.IsFalse(syntax.IsPrimitiveType);
            Assert.IsFalse(syntax.HasNamespace);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsFalse(syntax.IsArrayType);
        }

        [DataTestMethod]
        [DataRow("MyNamespace:MyType", "MyType", 1)]
        [DataRow("MyNamespace1:MyNamespace2:MyType", "MyType", 2)]
        [DataRow("MyNamespace1:MyNamespace2:MyNamespace3:MyType", "MyType", 3)]
        public void UserType_Namespace(string input, string identifier, int namespaceDepth)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsFalse(syntax.IsPrimitiveType);
            Assert.IsTrue(syntax.HasNamespace);
            Assert.AreEqual(namespaceDepth, syntax.NamespaceDepth);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsFalse(syntax.IsArrayType);
        }

        [DataTestMethod]
        [DataRow("MyParentType<i8>.MyType", "MyType", 1)]
        [DataRow("MyParentType<i8, i16>.MySubType<float>.MyType", "MyType", 2)]
        [DataRow("MyParentType<i8, float, SomeOther>.MyType2<Something>.MyType3<string>.MyType", "MyType", 3)]
        public void UserType_Nested(string input, string identifier, int nestedDepth)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsFalse(syntax.IsPrimitiveType);
            Assert.IsFalse(syntax.HasNamespace);
            Assert.IsTrue(syntax.IsNested);
            Assert.AreEqual(nestedDepth, syntax.NestedDepth);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsFalse(syntax.IsArrayType);
        }

        [DataTestMethod]
        [DataRow("MyType<i8>", "MyType", 1)]
        [DataRow("MyType<i8, string>", "MyType", 2)]
        [DataRow("MyType<i8, float, double>", "MyType", 3)]
        [DataRow("MyType<OtherType, float, double>", "MyType", 3)]
        [DataRow("MyType<SomeType.OtherType>", "MyType", 1)]
        [DataRow("MyType<SomeType.OtherType<i8, i16>>", "MyType", 1)]
        [DataRow("MyType<SomeType.OtherType<i8, i16>, Other<MyType>>", "MyType", 2)]
        public void UserType_Generic(string input, string identifier, int genericArguments)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsFalse(syntax.IsPrimitiveType);
            Assert.IsFalse(syntax.HasNamespace);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsTrue(syntax.IsGenericType);
            Assert.AreEqual(genericArguments, syntax.GenericArgumentCount);
            Assert.IsFalse(syntax.IsArrayType);
        }

        [DataTestMethod]
        [DataRow("MyType[]")]
        [DataRow("AnotherUserType[]")]
        public void UserType_Array_1(string input)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(input.TrimEnd('[', ']', ','), syntax.Identifier.Text);
            Assert.IsFalse(syntax.IsPrimitiveType);
            Assert.IsFalse(syntax.HasNamespace);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsTrue(syntax.IsArrayType);
            Assert.AreEqual(1, syntax.ArrayParameterRank);
        }

        [DataTestMethod]
        [DataRow("MyType[,]")]
        [DataRow("AnotherUserType[,]")]
        public void UserType_Array_2(string input)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(input.TrimEnd('[', ']', ','), syntax.Identifier.Text);
            Assert.IsFalse(syntax.IsPrimitiveType);
            Assert.IsFalse(syntax.HasNamespace);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsTrue(syntax.IsArrayType);
            Assert.AreEqual(2, syntax.ArrayParameterRank);
        }

        [DataTestMethod]
        [DataRow("MyType[,,]")]
        [DataRow("AnotherUserType[,,]")]
        public void UserType_Array_3(string input)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(input.TrimEnd('[', ']', ','), syntax.Identifier.Text);
            Assert.IsFalse(syntax.IsPrimitiveType);
            Assert.IsFalse(syntax.HasNamespace);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsTrue(syntax.IsArrayType);
            Assert.AreEqual(3, syntax.ArrayParameterRank);
        }

        [DataTestMethod]
        [DataRow("MyNamespace:MyType<i32>", "MyType", 1, 1)]
        [DataRow("MyNamespace1:MyNamespace2:MyType<float, MyOtherType>", "MyType", 2, 2)]
        [DataRow("MyNamespace1:MyNamespace2:MyNamespace3:MyType<double, string, Type<i8, i16>>", "MyType", 3, 3)]
        public void UserType_Namespace_Generic(string input, string identifier, int namespaceDepth, int genericArguments)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsFalse(syntax.IsPrimitiveType);
            Assert.IsTrue(syntax.HasNamespace);
            Assert.AreEqual(namespaceDepth, syntax.NamespaceDepth);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsTrue(syntax.IsGenericType);
            Assert.AreEqual(syntax.GenericArgumentCount, genericArguments);
            Assert.IsFalse(syntax.IsArrayType);
        }

        [DataTestMethod]
        [DataRow("MyNamespace:MyType[]", "MyType", 1)]
        [DataRow("MyNamespace1:MyNamespace2:MyType[]", "MyType", 2)]
        [DataRow("MyNamespace1:MyNamespace2:MyNamespace3:MyType[]", "MyType", 3)]
        public void UserType_Namespace_Array_1(string input, string identifier, int namespaceDepth)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsFalse(syntax.IsPrimitiveType);
            Assert.IsTrue(syntax.HasNamespace);
            Assert.AreEqual(namespaceDepth, syntax.NamespaceDepth);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsTrue(syntax.IsArrayType);
            Assert.AreEqual(1, syntax.ArrayParameterRank);
        }

        [DataTestMethod]
        [DataRow("MyNamespace:MyType[,]", "MyType", 1)]
        [DataRow("MyNamespace1:MyNamespace2:MyType[,]", "MyType", 2)]
        [DataRow("MyNamespace1:MyNamespace2:MyNamespace3:MyType[,]", "MyType", 3)]
        public void UserType_Namespace_Array_2(string input, string identifier, int namespaceDepth)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsFalse(syntax.IsPrimitiveType);
            Assert.IsTrue(syntax.HasNamespace);
            Assert.AreEqual(namespaceDepth, syntax.NamespaceDepth);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsTrue(syntax.IsArrayType);
            Assert.AreEqual(2, syntax.ArrayParameterRank);
        }

        [DataTestMethod]
        [DataRow("MyNamespace:MyType[,,]", "MyType", 1)]
        [DataRow("MyNamespace1:MyNamespace2:MyType[,,]", "MyType", 2)]
        [DataRow("MyNamespace1:MyNamespace2:MyNamespace3:MyType[,,]", "MyType", 3)]
        public void UserType_Namespace_Array_3(string input, string identifier, int namespaceDepth)
        {
            LumaSharpParser.TypeReferenceContext context = TestUtils.ParseTypeReference(input);

            // Build AST node
            TypeReferenceSyntax syntax = new TypeReferenceSyntax(null, null, context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsFalse(syntax.IsPrimitiveType);
            Assert.IsTrue(syntax.HasNamespace);
            Assert.AreEqual(namespaceDepth, syntax.NamespaceDepth);
            Assert.IsFalse(syntax.IsNested);
            Assert.IsFalse(syntax.IsGenericType);
            Assert.IsTrue(syntax.IsArrayType);
            Assert.AreEqual(3, syntax.ArrayParameterRank);
        }
        #endregion
    }
}
