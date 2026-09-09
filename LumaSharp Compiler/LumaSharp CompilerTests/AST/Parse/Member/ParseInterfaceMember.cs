using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Member
{
    [TestClass]
    public class ParseInterfaceMember
    {
        // Methods
        [DataTestMethod]
        [DataRow("interface a{}")]
        [DataRow("public interface b{}")]
        [DataRow(" interface c{}")]
        [DataRow(" internal interface d{}")]
        [DataRow("interface f : other{}")]
        [DataRow("interface g : some, extra{}")]
        [DataRow("interface h<T>{}")]
        [DataRow("interface i<T0, T1>{}")]
        public void ParseAsContractMember(string input)
        {
            // Try to parse the tree
            MemberSyntax member = TestUtils.ParseMemberDeclaration(input);

            Assert.IsNotNull(member);
            Assert.IsInstanceOfType(member, typeof(InterfaceSyntax));
        }
    }
}
