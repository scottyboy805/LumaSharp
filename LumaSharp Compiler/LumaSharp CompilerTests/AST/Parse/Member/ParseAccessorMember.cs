using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Member
{
    [TestClass]
    public class ParseAccessorMember
    {
        // Methods
        [DataTestMethod]
        [DataRow("i32 a=>null;")]
        [DataRow("export MyType b=>null;")]
        [DataRow("#copy string c=>null;")]
        [DataRow("#copy export SomeType d=>null;")]
        [DataRow("i32 e=>read:return null;")]
        [DataRow("i32 f=>write:return null;")]
        [DataRow("i32 g=>read:{return null;}")]
        [DataRow("i32 h=>write:{return null;}")]
        [DataRow("i32 i=>write:{return null;}=>read:return 0;")]
        [DataRow("i32 j=>write:{return null;}=>read:{return 0;}")]
        public void ParseAsAccessorMember(string input)
        {
            // Try to parse the tree
            MemberSyntax member = TestUtils.ParseMemberDeclaration(input);

            Assert.IsNotNull(member);
            Assert.IsInstanceOfType(member, typeof(AccessorSyntax));
        }
    }
}
