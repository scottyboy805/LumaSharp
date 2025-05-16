using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.AST
{
    [TestClass]
    public class FieldSyntaxTests
    {
        [DataTestMethod]
        [DataRow("i8 myField", "myField")]
        [DataRow("i16 _MyType = 0;", "_MyType")]
        [DataRow("i32 MyType123", "MyType123")]
        [DataRow("string _MyType123", "_MyType123")]
        public void Field(string input, string identifier)
        {
            // Build AST node
            FieldSyntax syntax = TestUtils.ParseFieldDeclaration(input);

            // Check for valid
            Assert.IsNotNull(syntax);
            Assert.AreEqual(identifier, syntax.Identifier.Text);
        }
    }
}
