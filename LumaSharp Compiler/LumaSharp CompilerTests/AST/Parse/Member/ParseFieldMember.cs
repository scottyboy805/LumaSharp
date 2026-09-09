using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Member
{
    [TestClass]
    public class ParseFieldMember
    {
        // Methods
        [DataTestMethod]
        [DataRow("i32 a;")]
        [DataRow("public MyType b;")]
        [DataRow("internal string c;")]
        [DataRow("protected internal SomeType d;")]
        [DataRow("f32 f = 0;")]
        [DataRow("char g = (1 + 2);")]
        public void ParseAsFieldMember(string input)
        {
            // Try to parse the tree
            MemberSyntax member = TestUtils.ParseMemberDeclaration(input);

            Assert.IsNotNull(member);
            Assert.IsInstanceOfType(member, typeof(FieldSyntax));
        }
    }
}
