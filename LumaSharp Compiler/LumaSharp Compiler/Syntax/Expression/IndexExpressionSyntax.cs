
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class IndexExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken lArray;
        private readonly SyntaxToken rArray;
        private readonly ExpressionSyntax accessExpression = null;
        private readonly SeparatedSyntaxList<ExpressionSyntax> indexExpressions;

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

        public SeparatedSyntaxList<ExpressionSyntax> IndexExpressions
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
        internal IndexExpressionSyntax(ExpressionSyntax accessExpression, SeparatedSyntaxList<ExpressionSyntax> indexExpressions)
            : this(
                  accessExpression,
                  Syntax.Token(SyntaxTokenKind.LArraySymbol),
                  indexExpressions,
                  Syntax.Token(SyntaxTokenKind.RArraySymbol))
        { 
        }

        internal IndexExpressionSyntax(ExpressionSyntax accessExpression, SyntaxToken lArray, SeparatedSyntaxList<ExpressionSyntax> indexExpressions, SyntaxToken rArray)
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

            // Set parent
            accessExpression.parent = this;
            indexExpressions.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitIndexExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitIndexExpression(this);
        }

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
