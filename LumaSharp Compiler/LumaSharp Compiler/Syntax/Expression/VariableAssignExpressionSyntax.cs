
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public enum AssignOperation
    {
        Assign,
        AddAssign,
        SubtractAssign,
        MultiplyAssign,
        DivideAssign,
    }

    public sealed class VariableAssignExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SeparatedSyntaxList<ExpressionSyntax> assignExpressions;
        private readonly SyntaxToken assign;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return assign; }
        }

        public override SyntaxToken EndToken
        {
            get { return assignExpressions.EndToken; }
        }

        public SyntaxToken Assign
        {
            get { return assign; }
        }

        public SeparatedSyntaxList<ExpressionSyntax> AssignExpressions
        {
            get { return assignExpressions; }
        }

        public bool HasAssignExpressions
        {
            get { return assignExpressions != null; }
        }

        public bool HasBlockAssignment
        {
            get { return assignExpressions.Count > 1; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return assignExpressions.Descendants; }
        }

        // Constructor
        internal VariableAssignExpressionSyntax(SeparatedSyntaxList<ExpressionSyntax> assignExpressions)
            : this(
                  new SyntaxToken(SyntaxTokenKind.AssignSymbol),
                  assignExpressions)
        {
        }

        internal VariableAssignExpressionSyntax(SyntaxToken assign, SeparatedSyntaxList<ExpressionSyntax> assignExpressions)
        {
            // Check kind
            if (assign.IsAssign == false)
                throw new ArgumentException(nameof(assign) + " must be a valid assign token");

            // Check null
            if(assignExpressions == null)
                throw new ArgumentNullException(nameof(assignExpressions));

            this.assign = assign;
            this.assignExpressions = assignExpressions;

            // Set parent
            assignExpressions.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write assign
            assign.GetSourceText(writer);

            // Assign expressions
            assignExpressions.GetSourceText(writer);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitVariableAssignExpression(this);
        }
    }
}
