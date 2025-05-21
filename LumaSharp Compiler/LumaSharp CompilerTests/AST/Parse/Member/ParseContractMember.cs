using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Member
{
    [TestClass]
    public class ParseContractMember
    {
        // Methods
        [DataTestMethod]
        [DataRow("contract a{}")]
        [DataRow("export contract b{}")]
        [DataRow("#copy contract c{}")]
        [DataRow("#copy export contract d{}")]
        [DataRow("contract f : other{}")]
        [DataRow("contract g : some, extra{}")]
        [DataRow("contract h<T>{}")]
        [DataRow("contract i<T0, T1>{}")]
        public void ParseAsContractMember(string input)
        {
            // Try to parse the tree
            MemberSyntax member = TestUtils.ParseMemberDeclaration(input);

            Assert.IsNotNull(member);
            Assert.IsInstanceOfType(member, typeof(ContractSyntax));
        }
    }
}
