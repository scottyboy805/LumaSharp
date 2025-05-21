using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Member
{
    [TestClass]
    public class ParseEnumMember
    {
        // Methods
        [DataTestMethod]
        [DataRow("enum a{}")]
        [DataRow("export enum b{}")]
        [DataRow("#copy enum c{}")]
        [DataRow("#copy export enum d{}")]
        [DataRow("enum f : other{}")]
        [DataRow("enum g : some, extra{}")]
        public void ParseAsEnumMember(string input)
        {
            // Try to parse the tree
            MemberSyntax member = TestUtils.ParseMemberDeclaration(input);

            Assert.IsNotNull(member);
            Assert.IsInstanceOfType(member, typeof(EnumSyntax));
        }
    }
}
