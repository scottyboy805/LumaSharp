using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Member
{
    [TestClass]
    public class ParseTypeMember
    {
        // Methods
        [DataTestMethod]
        [DataRow("type a{}")]
        [DataRow("export type b{}")]
        [DataRow("#copy type c{}")]
        [DataRow("#copy export type d{}")]
        [DataRow("type e override{}")]
        [DataRow("type f : other{}")]
        [DataRow("type g : some, extra{}")]
        [DataRow("type h<T>{}")]
        [DataRow("type i<T0, T1>{}")]
        public void ParseAsTypeMember(string input)
        {
            // Try to parse the tree
            MemberSyntax member = TestUtils.ParseMemberDeclaration(input);

            Assert.IsNotNull(member);
            Assert.IsInstanceOfType(member, typeof(TypeSyntax));
        }
    }
}
