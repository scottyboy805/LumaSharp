using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseTypeofExpression
    {
        // Methods
        [DataTestMethod]
        [DataRow("typeof(i32)")]
        [DataRow("typeof ( i32 )")]
        [DataRow("typeof\t(\ti32\t)\t")]
        [DataRow("typeof(SomeType)")]
        public void ParseAsTypeofExpression(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(TypeofExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("typeof(i32)")]
        public void ParseTypeofNode(string input)
        {
            // Try to parse the tree
            TypeofExpressionSyntax expression = TestUtils.ParseInputStringExpression(input,
                p => p.ParseTypeofExpression() as TypeofExpressionSyntax);

            Assert.IsNotNull(expression);
            Assert.AreEqual(SyntaxTokenKind.TypeofKeyword, expression.Keyword.Kind);
            Assert.AreEqual(SyntaxTokenKind.LParenSymbol, expression.LParen.Kind);
            Assert.AreEqual(SyntaxTokenKind.RParenSymbol, expression.RParen.Kind);
            Assert.AreEqual(SyntaxTokenKind.TypeofKeyword, expression.StartToken.Kind);
            Assert.AreEqual(SyntaxTokenKind.RParenSymbol, expression.EndToken.Kind);
            Assert.AreEqual(1, expression.Descendants.Count());
        }
    }
}
