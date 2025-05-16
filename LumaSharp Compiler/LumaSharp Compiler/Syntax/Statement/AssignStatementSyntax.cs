
namespace LumaSharp.Compiler.AST
{
    public sealed class AssignStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SeparatedListSyntax<ExpressionSyntax> left;
        private readonly VariableAssignExpressionSyntax variableAssign;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return left.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return variableAssign.EndToken; }
        }

        public VariableAssignExpressionSyntax AssignExpression
        {
            get { return variableAssign; }
        }

        public SeparatedListSyntax<ExpressionSyntax> Left
        {
            get { return left; }
        }

        public SeparatedListSyntax<ExpressionSyntax> Right
        {
            get { return variableAssign.AssignExpressions; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return left;
                yield return variableAssign;
            }
        }

        // Constructor
        internal AssignStatementSyntax(SeparatedListSyntax<ExpressionSyntax> left, VariableAssignExpressionSyntax assign)
        {
            // Check null
            if(left == null)
                throw new ArgumentNullException(nameof(left));

            if(assign == null)
                throw new ArgumentNullException(nameof(assign));

            this.left = left;
            this.variableAssign = assign;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write left
            left.GetSourceText(writer);

            // Write assign
            variableAssign.GetSourceText(writer);
        }
    }
}
