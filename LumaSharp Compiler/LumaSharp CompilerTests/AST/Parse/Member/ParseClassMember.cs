using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Member
{
    [TestClass]
    public class ParseClassMember
    {
        // Methods
        [DataTestMethod]
        [DataRow("class a{}")]
        [DataRow("public class b{}")]
        [DataRow("internal class c {}")]
        [DataRow("public static virtual class d {}")]
        [DataRow("virtual class e {}")]
        [DataRow("class f : other{}")]
        [DataRow("class g : some, extra{}")]
        [DataRow("class h<T>{}")]
        [DataRow("class i<T0, T1>{}")]
        public void ParseAsClassMember(string input)
        {
            // Try to parse the tree
            MemberSyntax member = TestUtils.ParseMemberDeclaration(input);

            Assert.IsNotNull(member);
            Assert.IsInstanceOfType(member, typeof(ClassSyntax));
        }
    }
}
