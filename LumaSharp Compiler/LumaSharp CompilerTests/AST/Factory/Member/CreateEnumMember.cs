using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Factory.Member
{
    [TestClass]
    public class CreateEnumMember
    {
        // Methods
        [TestMethod]
        public void CreateEnum()
        {
            EnumSyntax e = Syntax.Enum("Test");

            Assert.IsNotNull(e);
            Assert.AreEqual("Test", e.Identifier);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateEnumInvalidIdentifier()
        {
            Syntax.Enum(Syntax.Token(SyntaxTokenKind.Invalid));
        }
    }
}
