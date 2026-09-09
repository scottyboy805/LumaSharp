using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Factory.Member
{
    [TestClass]
    public class CreateContractMember
    {
        // Methods
        [TestMethod]
        public void CreateContract()
        {
            InterfaceSyntax contract = Syntax.Interface("Test");

            Assert.IsNotNull(contract);
            Assert.AreEqual("Test", contract.Identifier);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateContractInvalidIdentifier()
        {
            Syntax.Interface(Syntax.Token(SyntaxTokenKind.Invalid));
        }
    }
}
