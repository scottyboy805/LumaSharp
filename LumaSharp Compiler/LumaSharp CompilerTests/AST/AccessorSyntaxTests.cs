using LumaSharp_Compiler.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST
{
    [TestClass]
    public class AccessorSyntaxTests
    {
        [DataTestMethod]
        [DataRow("i32 myAccessor => 0;", "myAccessor")]
        [DataRow("u32 myAccessor => 20U;", "myAccessor")]
        [DataRow("i64 myAccessor => 40L;", "myAccessor")]
        [DataRow("u64 myAccessor => 60UL;", "myAccessor")]
        [DataRow("i32 myAccessor => 0xaf00ba36;", "myAccessor")]
        [DataRow("float myAccessor => 123.456;", "myAccessor")]
        [DataRow("float myAccessor => 12.45F;", "myAccessor")]
        [DataRow("double myAccessor => 12.45D;", "myAccessor")]
        [DataRow(@"string myAccessor => ""Hello World"";", "myAccessor")]
        [DataRow("i32 myAccessor => someIdentifier;", "myAccessor")]
        [DataRow("bool myAccessor => true;", "myAccessor")]
        [DataRow("bool myAccessor => false;", "myAccessor")]
        [DataRow("any myAccessor => null;", "myAccessor")]
        public void Accessor(string input, string identifier)
        {
            LumaSharpParser.AccessorDeclarationContext context = TestUtils.ParseAccessorDeclaration(input);

            // Build AST node
            AccessorSyntax syntax = new AccessorSyntax(context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
            Assert.IsTrue(syntax.HasAssignValue);
            Assert.IsFalse(syntax.HasAssignIdentifier);
            Assert.IsFalse(syntax.HasReadBody);
            Assert.IsFalse(syntax.HasWriteBody);
        }

        [DataTestMethod]
        [DataRow("i16 myAccessor => someField;", "myAccessor")]
        [DataRow("i32 myAccessor => someField;", "myAccessor")]
        [DataRow("string myAccessor => someField;", "myAccessor")]
        public void Accessor_Field(string input, string identifier)
        {
            LumaSharpParser.AccessorDeclarationContext context = TestUtils.ParseAccessorDeclaration(input);

            // Build AST node
            AccessorSyntax syntax = new AccessorSyntax(context);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
        }
    }
}
