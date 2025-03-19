
namespace LumaSharp.Compiler.AST
{
    public sealed class AssignStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken assign;
        private readonly SeparatedListSyntax<ExpressionSyntax> left = null;
        private readonly SeparatedListSyntax<ExpressionSyntax> right = null;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return left.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return right.EndToken; }
        }

        public SyntaxToken Assign
        {
            get { return assign; }
        }

        public SeparatedListSyntax<ExpressionSyntax> Left
        {
            get { return left; }
        }

        public SeparatedListSyntax<ExpressionSyntax> Right
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
        internal AssignStatementSyntax(SyntaxNode parent, AssignOperation op, ExpressionSyntax[] left, ExpressionSyntax[] right)
            : base(parent)
        {
            this.assign = Syntax.AssignOp(op);
            this.left = new SeparatedListSyntax<ExpressionSyntax>(this, SyntaxTokenKind.CommaSymbol);
            this.right = new SeparatedListSyntax<ExpressionSyntax>(this, SyntaxTokenKind.CommaSymbol);

            // Add left
            foreach (ExpressionSyntax expression in left)
                this.left.AddElement(expression, Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol));

            // Add right
            foreach (ExpressionSyntax expression in right)
                this.right.AddElement(expression, Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol));
        }

        internal AssignStatementSyntax(SyntaxNode parent, LumaSharpParser.AssignStatementContext assign)
            : base(parent)
        {
            // Op
            this.assign = new SyntaxToken(SyntaxTokenKind.AssignOperator, assign.ASSIGN());

            // Get left
            this.left = ExpressionSyntax.List(this, assign.expressionList(0));

            // Get right
            this.right = ExpressionSyntax.List(this, assign.expressionList(1));
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write left
            left.GetSourceText(writer);

            // Write op
            assign.GetSourceText(writer);

            // Write right
            right.GetSourceText(writer);
        }
    }
}
