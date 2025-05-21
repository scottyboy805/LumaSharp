using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseVariableReferenceExpression
    {
        // Methods
        [DataTestMethod]
        [DataRow("someVariable")]
        [DataRow("parameter")]
        [DataRow("_param")]
        [DataRow("_184774_3456")]
        public void ParseAsVariableReferenceExpression(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(VariableReferenceExpressionSyntax));
        }
    }
}
