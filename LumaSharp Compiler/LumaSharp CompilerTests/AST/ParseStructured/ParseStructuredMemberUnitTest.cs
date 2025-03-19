using LumaSharp.Compiler.AST;
using LumaSharp.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST.ParseStructured
{
    [TestClass]
    public class ParseStructuredMemberUnitTest
    {
        [DataTestMethod]
        [DataRow("type MyType{i32 myField;}", new object[] { false, false, 0 })]
        [DataRow("type MyType{export i32 myField;}", new object[] { true, false, 0 })]
        [DataRow("type MyType{hidden i32 myField;}", new object[] { true, false, 0 })]
        [DataRow("type MyType{internal i32 myField;}", new object[] { true, false, 0 })]
        [DataRow("type MyType{export i32 myField = 5;}", new object[] { true, true, 0 })]
        [DataRow("type MyType{i32 myField = 5;}", new object[] { false, true, 0 })]

        [DataRow("type MyType{#Tag i32 myField;}", new object[] { false, false, 1 })]
        [DataRow("type MyType{#Tag(1) export i32 myField;}", new object[] { true, false, 1 })]
        [DataRow("type MyType{#Tag #Range(1, 2) hidden i32 myField;}", new object[] { true, false, 2 })]
        [DataRow("type MyType{#Tag internal i32 myField;}", new object[] { true, false, 1 })]
        [DataRow("type MyType{#Range(false) export i32 myField = 5;}", new object[] { true, true, 1 })]
        [DataRow("type MyType{#Tag #Range(1, 2) i32 myField = 5;}", new object[] { false, true, 2 })]
        public void StructuredMember_Field(string input, bool hasModifiers, bool hasAssign, int attributeCount)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);

            FieldSyntax field = tree.DescendantsOfType<FieldSyntax>(true).First();
            Assert.IsNotNull(field);
            Assert.AreEqual("myField", field.Identifier.Text);
            Assert.AreEqual("i32", field.FieldType.Identifier.Text);
            Assert.AreEqual(hasModifiers, field.HasAccessModifiers);
            Assert.AreEqual(hasAssign, field.HasFieldAssignment);
            Assert.AreEqual(attributeCount, field.AttributeCount);
        }

        [DataTestMethod]
        [DataRow("type MyType{i32 myAccessor => read: return myFloatVal;}", new object[] { false, true, false, false, 0 })]
        [DataRow("type MyType{i32 myAccessor => read:{ return myFloatVal;}}", new object[] { false, true, false, false, 0 })]
        [DataRow("type MyType{i32 myAccessor => write: myFloatVal = 5;}", new object[] { false, false, true, false, 0 })]
        [DataRow("type MyType{i32 myAccessor => write: {myFloatVal = 5;}}", new object[] { false, false, true, false, 0 })]
        [DataRow("type MyType{i32 myAccessor => 5;", new object[] { false, false, false, true, 0 })]

        [DataRow("type MyType{internal i32 myAccessor => read: return myFloatVal;}", new object[] { true, true, false, false, 0 })]
        [DataRow("type MyType{internal i32 myAccessor => read:{ return myFloatVal;}}", new object[] { true, true, false, false, 0 })]
        [DataRow("type MyType{internal i32 myAccessor => write: myFloatVal = 5;}", new object[] { true, false, true, false, 0 })]
        [DataRow("type MyType{internal i32 myAccessor => write: {myFloatVal = 5;}}", new object[] { true, false, true, false, 0 })]
        [DataRow("type MyType{internal i32 myAccessor => read: return myFloatVal; => write: val = 5;}", new object[] { true, true, true, false, 0 })]
        [DataRow("type MyType{internal i32 myAccessor => read:{ return myFloatVal;} => write:{val = 5;}}", new object[] { true, true, true, false, 0 })]
        [DataRow("type MyType{internal i32 myAccessor => write: myFloatVal = 5;} => read: return 4;", new object[] { true, true, true, false, 0 })]
        [DataRow("type MyType{internal i32 myAccessor => write: {myFloatVal = 5;} => read: { return 4;}}", new object[] { true, true, true, false, 0 })]

        [DataRow("type MyType{hidden i32 myAccessor => read: return myFloatVal;}", new object[] { true, true, false, false, 0 })]
        [DataRow("type MyType{hidden i32 myAccessor => read:{ return myFloatVal;}}", new object[] { true, true, false, false, 0 })]
        [DataRow("type MyType{hidden i32 myAccessor => write: myFloatVal = 5;}", new object[] { true, false, true, false, 0 })]
        [DataRow("type MyType{hidden i32 myAccessor => write: {myFloatVal = 5;}}", new object[] { true, false, true, false, 0 })]

        [DataRow("type MyType{export i32 myAccessor => read: return myFloatVal;}", new object[] { true, true, false, false, 0 })]
        [DataRow("type MyType{export i32 myAccessor => read:{ return myFloatVal;}}", new object[] { true, true, false, false, 0 })]
        [DataRow("type MyType{export i32 myAccessor => write: myFloatVal = 5;}", new object[] { true, false, true, false, 0 })]
        [DataRow("type MyType{export i32 myAccessor => write: {myFloatVal = 5;}}", new object[] { true, false, true, false, 0 })]
        public void StructuredMember_Accessor(string input, bool hasModifiers, bool hasRead, bool hasWrite, bool hasExpression, int attributeCount)
        {
            // Try to parse the tree
            SyntaxTree tree = SyntaxTree.Parse(InputSource.FromSourceText(input));

            Assert.IsNotNull(tree);
            Assert.AreEqual(1, tree.RootElementCount);

            AccessorSyntax accessor = tree.DescendantsOfType<AccessorSyntax>(true).First();
            Assert.IsNotNull(accessor);
            Assert.AreEqual("myAccessor", accessor.Identifier.Text);
            Assert.AreEqual("i32", accessor.AccessorType.Identifier.Text);
            Assert.AreEqual(hasModifiers, accessor.HasAccessModifiers);
            Assert.AreEqual(hasRead, accessor.HasReadBody);
            Assert.AreEqual(hasWrite, accessor.HasWriteBody);
            Assert.AreEqual(hasExpression, accessor.HasLambdaBody);
            Assert.AreEqual(attributeCount, accessor.AttributeCount);
        }
    }
}
