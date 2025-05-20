
namespace LumaSharp.Compiler.AST
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly VariableDeclarationStatementSyntax variable;
        private readonly ExpressionSyntax condition;
        private readonly SeparatedSyntaxList<ExpressionSyntax> increments;
        private readonly StatementSyntax statement;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return statement.EndToken; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public VariableDeclarationStatementSyntax Variable
        {
            get { return variable; }
        }

        public ExpressionSyntax Condition
        {
            get { return condition; }
        }

        public SeparatedSyntaxList<ExpressionSyntax> Increments
        {
            get { return increments; }
        }

        public StatementSyntax Statement
        {
            get { return statement; }
        }

        public bool HasVariable
        { 
            get { return variable != null; }
        }

        public bool HasCondition
        {
            get { return condition != null; }
        }

        public bool HasIncrements
        {
            get { return increments != null; }
        }

        // Constructor
        internal ForStatementSyntax(VariableDeclarationStatementSyntax variable, ExpressionSyntax condition, SeparatedSyntaxList<ExpressionSyntax> increments, StatementSyntax statement)
            : this(
                  new SyntaxToken(SyntaxTokenKind.ForKeyword),
                  variable,
                  condition,
                  increments,
                  statement)
        {
        }

        internal ForStatementSyntax(SyntaxToken keyword, VariableDeclarationStatementSyntax variable, ExpressionSyntax condition, SeparatedSyntaxList<ExpressionSyntax> increments, StatementSyntax statement)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.ForKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.ForKeyword);

            // Check null
            if(statement == null)
                throw new ArgumentNullException(nameof(statement));

            this.keyword = keyword;
            this.variable = variable;
            this.condition = condition;
            this.increments = increments;
            this.statement = statement;

            // Set parent
            if (variable != null) variable.parent = this;
            if (condition != null) condition.parent = this;
            if (increments != null) increments.parent = this;
            statement.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Variable
            if(HasVariable == true)
                variable.GetSourceText(writer);

            // Foreach condition
            if(HasCondition == true)
                condition.GetSourceText(writer);

            // For expression
            if (HasIncrements == true) 
                increments.GetSourceText(writer);

            // Statement
            statement.GetSourceText(writer);
        }
    }
}
