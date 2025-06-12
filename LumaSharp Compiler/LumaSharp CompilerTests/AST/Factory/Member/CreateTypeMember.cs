using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Factory.Member
{
    [TestClass]
    public class CreateTypeMember
    {
        // Methods
        [TestMethod]
        public void CreateType()
        {
            TypeSyntax type = Syntax.Type("Test");

            Assert.IsNotNull(type);
            Assert.AreEqual("Test", type.Identifier);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateTypeInvalidIdentifier()
        {
            Syntax.Type(Syntax.Token(SyntaxTokenKind.Invalid));
        }
    }
}
