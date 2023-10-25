
using Antlr4.Runtime;
using LumaSharp_Compiler.AST.Expression;
using System.Linq.Expressions;

namespace LumaSharp_Compiler.AST
{
    public sealed class ArrayIndexExpressionSyntax : ExpressionSyntax
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

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return accessExpression;

                // Get index expressions
                foreach (SyntaxNode node in indexExpressions)
                    yield return node;
            }
        }

        // Constructor
        internal ArrayIndexExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.IndexExpressionContext expression)
            : base(tree, parent, expression)
        {
            this.lArray = new SyntaxToken(expression.larray);
            this.rArray = new SyntaxToken(expression.rarray);

            // Create index expressions
            this.indexExpressions = expression.expression().Select(e => Any(tree, this, e)).ToArray();

            // Not sure if ths is correct??
            this.accessExpression = parent as ExpressionSyntax;
        }

        internal ArrayIndexExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(tree, parent, expression)
        {
            // Get index
            LumaSharpParser.IndexExpressionContext[] indexes = expression.indexExpression();
            LumaSharpParser.IndexExpressionContext index = indexes[indexes.Length - 1];


            this.lArray = new SyntaxToken(index.larray);
            this.rArray = new SyntaxToken(index.rarray);

            // Create index expressions
            this.indexExpressions = index.expression().Select(e => Any(tree, this, e)).ToArray();

            // Create access expression
            // Check for nested indexes
            if(indexes.Length > 1)
            {
                ArrayIndexExpressionSyntax[] nestedIndexes = new ArrayIndexExpressionSyntax[indexes.Length - 1];

                for(int i = 0; i < indexes.Length; i++)
                {

                }
            }

            // Check for nested index
            //if (expression.indexExpression() != null)
            //    this.accessExpression = new ArrayIndexExpressionSyntax(tree, this, expression.indexExpression());

            // Check for identifier
            if (expression.IDENTIFIER() != null)
                this.accessExpression = new VariableReferenceExpressionSyntax(tree, this, expression);

            // Check for literal
            if (expression.endExpression() != null)
                this.accessExpression = new LiteralExpressionSyntax(tree, this, expression.endExpression());

            // Check for method
            if (expression.methodInvokeExpression() != null)
                this.accessExpression = new MethodInvokeExpressionSyntax(tree, this, expression);

            // Check for field
            if (expression.fieldAccessExpression() != null)
                this.accessExpression = new FieldAccessorReferenceExpressionSyntax(tree, this, expression);

            // Check for new
            if(expression.newExpression() != null)
                this.accessExpression = new NewExpressionSyntax(tree, this, expression.newExpression());

            // Check for initializer
            if (expression.initializerInvokeExpression() != null)
                this.accessExpression = new NewExpressionSyntax(tree, this, expression.initializerInvokeExpression());

            // Check for base
            if (expression.BASE() != null)
                this.accessExpression = new BaseExpressionSyntax(tree, this, expression);

            // Check for this
            if(expression.THIS() != null)
                this.accessExpression = new ThisExpressionSyntax(tree, this, expression);

            // Check for paren
            if (expression.lparen != null)
                this.accessExpression = Any(tree, this, expression.expression(0));
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
