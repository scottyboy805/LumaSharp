using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Member
{
    [TestClass]
    public class ParseConstructorMember
    {
        // Methods
        [DataTestMethod]
        [DataRow("this(){}")]
        [DataRow("export this(){}")]
        [DataRow("#copy export this(){}")]
        [DataRow("this():base(){}")]
        [DataRow("this():base(5){}")]
        [DataRow("this():this(){}")]
        [DataRow("this():this(10){}")]
        [DataRow("this()=>return;")]
        [DataRow("this():base()=>return;")]
        public void ParseAsConstructorMember(string input)
        {
            // Try to parse the tree
            MemberSyntax member = TestUtils.ParseMemberDeclaration(input);

            Assert.IsNotNull(member);
            Assert.IsInstanceOfType(member, typeof(ConstructorSyntax));
        }
    }
}
