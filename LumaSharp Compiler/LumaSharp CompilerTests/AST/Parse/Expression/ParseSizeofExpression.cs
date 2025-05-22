using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Expression
{
    [TestClass]
    public class ParseSizeofExpression
    {
        // Methods
        [DataTestMethod]
        [DataRow("sizeof(i32)")]
        [DataRow("sizeof ( i32 )")]
        [DataRow("sizeof\t(\ti32\t)\t")]
        [DataRow("sizeof(SomeType)")]
        public void ParseAsSizeofExpression(string input)
        {
            // Try to parse the tree
            ExpressionSyntax expression = TestUtils.ParseInputStringExpression(input);

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(SizeofExpressionSyntax));
        }

        [DataTestMethod]
        [DataRow("sizeof(i32)")]
        public void ParseSizeofNode(string input)
        {
            // Try to parse the tree
            SizeofExpressionSyntax expression = TestUtils.ParseInputStringExpression(input,
                p => p.ParseSizeofExpression() as SizeofExpressionSyntax);

            Assert.IsNotNull(expression);
            Assert.AreEqual(SyntaxTokenKind.SizeofKeyword, expression.Keyword.Kind);
            Assert.AreEqual(SyntaxTokenKind.LParenSymbol, expression.LParen.Kind);
            Assert.AreEqual(SyntaxTokenKind.RParenSymbol, expression.RParen.Kind);
            Assert.AreEqual(SyntaxTokenKind.SizeofKeyword, expression.StartToken.Kind);
            Assert.AreEqual(SyntaxTokenKind.RParenSymbol, expression.EndToken.Kind);
            Assert.AreEqual(1, expression.Descendants.Count());
        }
    }
}
