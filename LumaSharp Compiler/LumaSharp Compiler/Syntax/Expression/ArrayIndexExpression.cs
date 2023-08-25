
namespace LumaSharp_Compiler.Syntax
{
    public sealed class ArrayIndexExpression : ExpressionSyntax
    {
        // Private
        private SyntaxToken lArray = null;
        private SyntaxToken rArray = null;
        private ExpressionSyntax accessExpression = null;
        private ExpressionSyntax[] indexExpressions = null;

        // Properties
        public ExpressionSyntax AccessExpression
        {
            get { return accessExpression; }
        }

        public ExpressionSyntax[] IndexExpressions
        {
            get { return indexExpressions; }
        }

        public int ArrayRank
        {
            get { return indexExpressions.Length; }
        }

        public override SyntaxToken StartToken
        {
            get { return accessExpression.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return rArray; }
        }

        // Constructor
        internal ArrayIndexExpression(SyntaxTree tree, SyntaxNode parent, ExpressionSyntax accessExpression, ExpressionSyntax[] indexExpressions)
            : base(tree, parent)
        {
            this.lArray = new SyntaxToken("[");
            this.rArray = new SyntaxToken("]");
            this.accessExpression = accessExpression;
            this.indexExpressions = indexExpressions;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write access
            accessExpression.GetSourceText(writer);

            // Write larray
            writer.Write(lArray.ToString());

            // Write all index
            for(int i = 0; i < indexExpressions.Length; i++)
            {
                indexExpressions[i].GetSourceText(writer);

                // Write comma
                if(i < indexExpressions.Length - 1) 
                    writer.Write(", ");
            }

            // Write rarray
            writer.Write(rArray.ToString());
        }
    }
}
