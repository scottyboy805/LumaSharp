using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Member
{
    [TestClass]
    public class ParseMethodMember
    {
        // Methods
        [DataTestMethod]
        [DataRow("i32 a(){}")]
        [DataRow("public MyType b(){}")]
        [DataRow("virtual string c(){}")]
        [DataRow("virtual public SomeType d(){}")]
        [DataRow("static i32 e(){}")]
        [DataRow("f32 f() => return null;")]
        [DataRow("void g(){}")]
        [DataRow("void h<T>(){}")]
        [DataRow("void i(i32 val){}")]
        [DataRow("void j<T>(T val){}")]
        [DataRow("virtual void k();")]
        public void ParseAsMethodMember(string input)
        {
            // Try to parse the tree
            MemberSyntax member = TestUtils.ParseMemberDeclaration(input);

            Assert.IsNotNull(member);
            Assert.IsInstanceOfType(member, typeof(MethodSyntax));
        }
    }
}
