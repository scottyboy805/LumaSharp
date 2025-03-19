
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
        internal ArrayIndexExpressionSyntax(SyntaxNode parent, ExpressionSyntax accessExpression, ExpressionSyntax[] indexExpressions)
            : base(parent, null)
        {
            this.lArray = Syntax.KeywordOrSymbol(SyntaxTokenKind.LArraySymbol);
            this.rArray = Syntax.KeywordOrSymbol(SyntaxTokenKind.RArraySymbol);

            this.accessExpression = accessExpression;
            this.indexExpressions = new SeparatedListSyntax<ExpressionSyntax>(this, SyntaxTokenKind.CommaSymbol);

            foreach (ExpressionSyntax expression in indexExpressions)
                this.indexExpressions.AddElement(expression, Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol));
        }

        internal ArrayIndexExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            // Get the index expression
            LumaSharpParser.IndexExpressionContext indexExpression = expression.indexExpression();

            this.lArray = new SyntaxToken(SyntaxTokenKind.LArraySymbol, indexExpression.LARRAY());
            this.rArray = new SyntaxToken(SyntaxTokenKind.RArraySymbol, indexExpression.RARRAY());

            // Access expression
            this.accessExpression = ExpressionSyntax.Any(this, expression.expression(0));

            // Get expression list
            LumaSharpParser.ExpressionListContext expressions = indexExpression.expressionList();

            // Check for any
            if(indexExpression != null)
            {
                // Create list
                this.indexExpressions = new SeparatedListSyntax<ExpressionSyntax>(this, SyntaxTokenKind.CommaSymbol);

                // Add primary
                this.indexExpressions.AddElement(
                    ExpressionSyntax.Any(this.indexExpressions, expressions.expression()), null);


                // Add secondary
                LumaSharpParser.ExpressionSecondaryContext[] secondaryExpressions = expressions.expressionSecondary();

                // Add all
                if(secondaryExpressions != null)
                {
                    foreach(LumaSharpParser.ExpressionSecondaryContext secondaryExpression in secondaryExpressions)
                    {
                        this.indexExpressions.AddElement(
                            ExpressionSyntax.Any(this.indexExpressions, secondaryExpression.expression()),
                            new SyntaxToken(SyntaxTokenKind.CommaSymbol, secondaryExpression.COMMA()));
                    }
                }
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write access
            accessExpression.GetSourceText(writer);

            // Write larray
            lArray.GetSourceText(writer);

            // Write all index
            indexExpressions.GetSourceText(writer);

            // Write rarray
            rArray.GetSourceText(writer);
        }
    }
}
