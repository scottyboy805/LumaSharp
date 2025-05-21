using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseMemberReferenceExpression
    {
        // Methods
        [DataTestMethod]
        [DataRow("i32.maxSize")]
        [DataRow("string.terminatingCharacter")]
        [DataRow("some.terminatingCharacter")]
        [DataRow("base.value")]
        [DataRow("this.something")]
        [DataRow("45.terminatingCharacter")]
        [DataRow("some[0].terminatingCharacter")]
        [DataRow("some.terminating.Character")]
        public void ParseAsMemberReference(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(MemberAccessExpressionSyntax));
        }
    }
}
