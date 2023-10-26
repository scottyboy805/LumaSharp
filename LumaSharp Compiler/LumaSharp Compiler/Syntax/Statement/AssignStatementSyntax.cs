
namespace LumaSharp_Compiler.AST.Statement
{
    public sealed class AssignStatementSyntax : StatementSyntax
    {
        // Private
        private SyntaxToken assignOperation = null;
        private ExpressionSyntax left = null;
        private ExpressionSyntax right = null;

        // Properties
        public SyntaxToken AssignOperation
        {
            get { return assignOperation; }
        }

        public ExpressionSyntax Left
        {
            get { return left; }
        }

        public ExpressionSyntax Right
        {
            get { return right; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return left;
                yield return right;
            }
        }

        // Constructor
        internal AssignStatementSyntax(ExpressionSyntax left, ExpressionSyntax right)
            : base(left.StartToken)
        {
            this.left = left;
            this.right = right;

            assignOperation = SyntaxToken.Assign();
        }

        internal AssignStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.AssignStatementContext assign)
            : base(tree, parent, assign)
        {
            // Op
            this.assignOperation = new SyntaxToken(assign.assign);

            // Get left
            this.left = ExpressionSyntax.Any(tree, this, assign.expression(0));

            // Get right
            this.right = ExpressionSyntax.Any(tree, this, assign.expression(1));

            // Semicolon
            this.statementEnd = new SyntaxToken(assign.semi);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write left
            left.GetSourceText(writer);

            // Write op
            assignOperation.GetSourceText(writer);

            // Write right
            right.GetSourceText(writer);

            // Semicolon
            statementEnd.GetSourceText(writer);
        }
    }
}
