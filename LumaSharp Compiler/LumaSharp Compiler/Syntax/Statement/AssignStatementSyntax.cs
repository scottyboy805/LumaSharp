
namespace LumaSharp_Compiler.Syntax.Statement
{
    public sealed class AssignStatementSyntax : StatementSyntax
    {
        // Private
        private SyntaxToken assignOperation = null;
        private ExpressionSyntax left = null;
        private ExpressionSyntax right = null;
        private SyntaxToken semicolon = null;

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
            this.semicolon = new SyntaxToken(assign.semi);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write left
            left.GetSourceText(writer);

            // Write op
            writer.Write(assignOperation.Text);

            // Write right
            right.GetSourceText(writer);

            // Semicolon
            writer.Write(semicolon.Text);
        }
    }
}
