
namespace LumaSharp.Compiler.AST
{
    public sealed class ArrayIndexExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken lArray;
        private readonly SyntaxToken rArray;
        private readonly ExpressionSyntax accessExpression = null;
        private readonly SeparatedListSyntax<ExpressionSyntax> indexExpressions;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lArray; }
        }

        public override SyntaxToken EndToken
        {
            get { return rArray; }
        }

        public SyntaxToken LArray
        {
            get { return lArray; }
        }

        public SyntaxToken RArray
        {
            get { return rArray; }
        }

        public ExpressionSyntax AccessExpression
        {
            get { return accessExpression; }
        }

        public SeparatedListSyntax<ExpressionSyntax> IndexExpressions
        {
            get { return indexExpressions; }
        }

        public int ArrayRank
        {
            get { return indexExpressions.Count; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return accessExpression;

                // Get index expressions
                foreach (SyntaxNode node in indexExpressions.Descendants)
                    yield return node;
            }
        }

        // Constructor
        internal ArrayIndexExpressionSyntax(ExpressionSyntax accessExpression, SeparatedListSyntax<ExpressionSyntax> indexExpressions)
            : this(
                  accessExpression,
                  new SyntaxToken(SyntaxTokenKind.LArraySymbol),
                  indexExpressions,
                  new SyntaxToken(SyntaxTokenKind.RArraySymbol))
        { 
        }

        internal ArrayIndexExpressionSyntax(ExpressionSyntax accessExpression, SyntaxToken lArray, SeparatedListSyntax<ExpressionSyntax> indexExpressions, SyntaxToken rArray)
        {
            // Check for null
            if(accessExpression == null)
                throw new ArgumentNullException(nameof(accessExpression));

            if(indexExpressions == null)
                throw new ArgumentNullException(nameof(indexExpressions));

            // Check for kind
            if(lArray.Kind != SyntaxTokenKind.LArraySymbol)
                throw new ArgumentException(nameof(lArray) + " must be of kind: " +  SyntaxTokenKind.LArraySymbol);

            if(rArray.Kind != SyntaxTokenKind.RArraySymbol)
                throw new ArgumentException(nameof(rArray) + " must be of kind: " + SyntaxTokenKind.RArraySymbol);

            this.lArray = lArray;
            this.rArray = rArray;
            this.accessExpression = accessExpression;
            this.indexExpressions = indexExpressions;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write access
            accessExpression.GetSourceText(writer);

            // Write lArray
            lArray.GetSourceText(writer);

            // Write all index
            indexExpressions.GetSourceText(writer);

            // Write rArray
            rArray.GetSourceText(writer);
        }
    }
}
