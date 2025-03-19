
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
        private readonly SeparatedListSyntax<ExpressionSyntax> assignExpressions;
        private readonly SyntaxToken assign;
        private readonly SyntaxToken lBlock;
        private readonly SyntaxToken rBlock;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return assign; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for block
                if (HasBlockAssignment == true)
                    return rBlock;

                // Assignment
                return assignExpressions.EndToken;
            }
        }

        public SyntaxToken Assign
        {
            get { return assign; }
        }

        public SyntaxToken LBlock
        {
            get { return lBlock; }
        }

        public SyntaxToken RBlock
        {
            get { return rBlock; }
        }

        public SeparatedListSyntax<ExpressionSyntax> AssignExpressions
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
        internal VariableAssignExpressionSyntax(SyntaxNode parent, AssignOperation op, ExpressionSyntax[] assignExpressions)
            : base(parent, null)
        {
            if (assignExpressions != null)
            {
                // Create assign
                this.assignExpressions = new SeparatedListSyntax<ExpressionSyntax>(this, SyntaxTokenKind.CommaSymbol);

                // Add all assigns
                foreach (ExpressionSyntax expression in assignExpressions)
                    this.assignExpressions.AddElement(expression, Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol));

                if (assignExpressions.Length > 1)
                {
                    this.lBlock = Syntax.KeywordOrSymbol(SyntaxTokenKind.LBlockSymbol);
                    this.rBlock = Syntax.KeywordOrSymbol(SyntaxTokenKind.RBlockSymbol);
                }
            }

            this.assign = Syntax.AssignOp(op);
        }

        internal VariableAssignExpressionSyntax(SyntaxNode parent, LumaSharpParser.VariableAssignmentContext assignment)
            : base(parent, null)
        {
            // Assign expressions
            this.assignExpressions = ExpressionSyntax.List(this, assignment.expressionList());

            // Assign
            this.assign = new SyntaxToken(SyntaxTokenKind.AssignOperator, assignment.ASSIGN());

            // Check for block
            if (assignment.LBLOCK() != null)
                this.lBlock = new SyntaxToken(SyntaxTokenKind.LBlockSymbol, assignment.LBLOCK());

            if (assignment.RBLOCK() != null)
                this.rBlock = new SyntaxToken(SyntaxTokenKind.RBlockSymbol, assignment.RBLOCK());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write assign
            assign.GetSourceText(writer);

            // Check for multiple expressions
            if (HasBlockAssignment == true)
                lBlock.GetSourceText(writer);

            // Assign expressions
            assignExpressions.GetSourceText(writer);

            // Check for multiple expressions
            if (HasBlockAssignment == true)
                rBlock.GetSourceText(writer);
        }
    }
}
