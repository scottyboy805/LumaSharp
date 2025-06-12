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
            ContractSyntax contract = Syntax.Contract("Test");

            Assert.IsNotNull(contract);
            Assert.AreEqual("Test", contract.Identifier);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateContractInvalidIdentifier()
        {
            Syntax.Contract(new SyntaxToken(SyntaxTokenKind.Invalid));
        }
    }
}
