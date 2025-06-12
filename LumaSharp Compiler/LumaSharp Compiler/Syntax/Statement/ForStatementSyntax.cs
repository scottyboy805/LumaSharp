

using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly VariableDeclarationSyntax variable;
        private readonly SyntaxToken variableSemicolon;
        private readonly ExpressionSyntax condition;
        private readonly SyntaxToken conditionSemicolon;
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

        public SyntaxToken VariableSemicolon
        {
            get { return variableSemicolon; }
        }

        public SyntaxToken ConditionSemicolon
        {
            get { return conditionSemicolon; }
        }

        public VariableDeclarationSyntax Variable
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

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                if(variable != null)
                    yield return variable;

                if(condition != null)
                    yield return condition;

                if (increments != null)
                    yield return increments;
            }
        }

        // Constructor
        internal ForStatementSyntax(VariableDeclarationSyntax variable, ExpressionSyntax condition, SeparatedSyntaxList<ExpressionSyntax> increments, StatementSyntax statement)
            : this(
                  Syntax.Token(SyntaxTokenKind.ForKeyword),
                  variable,
                  Syntax.Token(SyntaxTokenKind.SemicolonSymbol),
                  condition,
                  Syntax.Token(SyntaxTokenKind.SemicolonSymbol),
                  increments,
                  statement)
        {
        }

        internal ForStatementSyntax(SyntaxToken keyword, VariableDeclarationSyntax variable, SyntaxToken variableSemicolon, ExpressionSyntax condition, SyntaxToken conditionSemicolon, SeparatedSyntaxList<ExpressionSyntax> increments, StatementSyntax statement)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.ForKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.ForKeyword);

            // Check null
            if(statement == null)
                throw new ArgumentNullException(nameof(statement));

            this.keyword = keyword;
            this.variable = variable;
            this.variableSemicolon = variableSemicolon;
            this.condition = condition;
            this.conditionSemicolon = conditionSemicolon;
            this.increments = increments;
            this.statement = statement;

            // Set parent
            if (variable != null) variable.parent = this;
            if (condition != null) condition.parent = this;
            if (increments != null) increments.parent = this;
            statement.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitForStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitForStatement(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Variable
            if(HasVariable == true)
                variable.GetSourceText(writer);

            // Variable semicolon
            variableSemicolon.GetSourceText(writer);

            // Foreach condition
            if(HasCondition == true)
                condition.GetSourceText(writer);

            // Condition semicolon
            conditionSemicolon.GetSourceText(writer);

            // For expression
            if (HasIncrements == true) 
                increments.GetSourceText(writer);

            // Statement
            statement.GetSourceText(writer);
        }
    }
}
