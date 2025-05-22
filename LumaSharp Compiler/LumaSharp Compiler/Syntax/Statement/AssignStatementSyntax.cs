
namespace LumaSharp.Compiler.AST
{
    public sealed class AssignStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SeparatedSyntaxList<ExpressionSyntax> left;
        private readonly VariableAssignmentExpressionSyntax variableAssign;
        private readonly SyntaxToken semicolon;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return left.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return semicolon; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        public VariableAssignmentExpressionSyntax AssignExpression
        {
            get { return variableAssign; }
        }

        public SeparatedSyntaxList<ExpressionSyntax> Left
        {
            get { return left; }
        }

        public SeparatedSyntaxList<ExpressionSyntax> Right
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
        internal AssignStatementSyntax(SeparatedSyntaxList<ExpressionSyntax> left, VariableAssignmentExpressionSyntax assign)
            : this(
                  left, 
                  assign,
                  new SyntaxToken(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal AssignStatementSyntax(SeparatedSyntaxList<ExpressionSyntax> left, VariableAssignmentExpressionSyntax assign, SyntaxToken semicolon)
        {
            // Check null
            if(left == null)
                throw new ArgumentNullException(nameof(left));

            if(assign == null)
                throw new ArgumentNullException(nameof(assign));

            // Check kind
            if (semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol); 

            this.left = left;
            this.variableAssign = assign;
            this.semicolon = semicolon;

            // Set parent
            left.parent = this;
            assign.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write left
            left.GetSourceText(writer);

            // Write assign
            variableAssign.GetSourceText(writer);

            // Semicolon
            semicolon.GetSourceText(writer);
        }
    }
}
